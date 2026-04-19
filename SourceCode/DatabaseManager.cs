using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace CST2550
{
	// This class handles everything to do with the database.
	// All SQL queries go through here - the controllers just call these methods.
	public class DatabaseManager
	{
		private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CarRecoveryDB;Integrated Security=True;TrustServerCertificate=True";


		// --- Schema setup (runs once on startup) ----------------------------

		// Creates CarRecoveryDB if it doesn't already exist.
		// Connects to master first (which always exists on any SQL Server install)
		// then creates our database from there. This means the app works on a
		// fresh machine without needing to set anything up in SSMS manually.
		public void EnsureDatabaseExists()
		{
			string masterConn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True";
			using SqlConnection conn = new SqlConnection(masterConn);
			conn.Open();
			string sql = "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CarRecoveryDB') CREATE DATABASE CarRecoveryDB";
			new SqlCommand(sql, conn).ExecuteNonQuery();
		}

		// Makes sure the Users table exists with all the right columns.
		// I also handle the case where the table already exists from a previous
		// version that didn't have the FullName or Role columns.
		public void EnsureAuthTableExists()
		{
			using SqlConnection conn = new SqlConnection(connectionString);
			conn.Open();

			// Create the table if it doesn't exist yet
			string createTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                CREATE TABLE Users (
                    Id           INT IDENTITY(1,1) PRIMARY KEY,
                    Username     NVARCHAR(100) NOT NULL UNIQUE,
                    PasswordHash NVARCHAR(256) NOT NULL,
                    FullName     NVARCHAR(200) NOT NULL DEFAULT '',
                    Role         NVARCHAR(50)  NOT NULL DEFAULT 'Operator',
                    CreatedAt    DATETIME      NOT NULL DEFAULT GETUTCDATE()
                )";
			new SqlCommand(createTable, conn).ExecuteNonQuery();

			// If the table already existed, add any missing columns
			AddColumnIfMissing(conn, "FullName", "ALTER TABLE Users ADD FullName NVARCHAR(200) NOT NULL DEFAULT ''");
			AddColumnIfMissing(conn, "Role", "ALTER TABLE Users ADD Role NVARCHAR(50) NOT NULL DEFAULT 'Operator'");
			AddColumnIfMissing(conn, "CreatedAt", "ALTER TABLE Users ADD CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()");

			// An older version of this project added Email and PhoneNumber as NOT NULL
			// which breaks our inserts - this makes them nullable so it doesn't crash
			MakeColumnNullableIfExists(conn, "Email", "NVARCHAR(200)");
			MakeColumnNullableIfExists(conn, "PhoneNumber", "NVARCHAR(50)");
		}

		// If BreakdownTime was created as DATE instead of DATETIME2, the time
		// part gets cut off when saving. This fixes that.
		public void EnsureCarRecoveriesSchema()
		{
			using SqlConnection conn = new SqlConnection(connectionString);
			conn.Open();

			string fixColumn = @"
                IF EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'CarRecoveries'
                      AND COLUMN_NAME = 'BreakdownTime'
                      AND DATA_TYPE IN ('date', 'smalldatetime')
                )
                ALTER TABLE CarRecoveries ALTER COLUMN BreakdownTime DATETIME2 NOT NULL";

			new SqlCommand(fixColumn, conn).ExecuteNonQuery();
		}

		// Helper - only adds a column if it isn't already there
		private void AddColumnIfMissing(SqlConnection conn, string columnName, string alterStatement)
		{
			string sql = $@"
                IF NOT EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = '{columnName}'
                )
                {alterStatement}";
			new SqlCommand(sql, conn).ExecuteNonQuery();
		}

		// Helper - makes a column nullable if it exists (to fix legacy schema issues)
		private void MakeColumnNullableIfExists(SqlConnection conn, string columnName, string dataType)
		{
			string sql = $@"
                IF EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = '{columnName}'
                )
                ALTER TABLE Users ALTER COLUMN {columnName} {dataType} NULL";
			new SqlCommand(sql, conn).ExecuteNonQuery();
		}


		// --- Recovery record operations -------------------------------------

		// Reads all records from the database and adds them into the BST.
		// This runs at startup so the tree is ready before any requests come in.
		public void LoadFromDatabase(RecoveryTree tree)
		{
			string query = "SELECT NumberPlate, CarModel, Issue, Location, BreakdownTime, Status FROM CarRecoveries";

			using SqlConnection conn = new SqlConnection(connectionString);
			SqlCommand cmd = new SqlCommand(query, conn);

			try
			{
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();

				while (reader.Read())
				{
					tree.Add(new RecoveryRecord(
						reader["NumberPlate"].ToString(),
						reader["CarModel"].ToString(),
						reader["Issue"].ToString(),
						reader["Location"].ToString(),
						(DateTime)reader["BreakdownTime"],
						reader["Status"].ToString()
					));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to load records from database: " + ex.Message);
			}
		}

		// Saves a new breakdown record to the database
		public void SaveToDatabase(RecoveryRecord record)
		{
			string query = @"INSERT INTO CarRecoveries 
                             (NumberPlate, CarModel, Issue, Location, BreakdownTime, Status) 
                             VALUES (@plate, @model, @issue, @location, @time, @status)";

			using SqlConnection conn = new SqlConnection(connectionString);
			SqlCommand cmd = new SqlCommand(query, conn);

			cmd.Parameters.AddWithValue("@plate", record.NumberPlate);
			cmd.Parameters.AddWithValue("@model", record.CarModel);
			cmd.Parameters.AddWithValue("@issue", record.Issue);
			cmd.Parameters.AddWithValue("@location", record.Location);
			cmd.Parameters.AddWithValue("@time", record.BreakdownTime);
			cmd.Parameters.AddWithValue("@status", record.Status);

			conn.Open();
			cmd.ExecuteNonQuery();
		}

		// Updates an existing record in the database - number plate is used to identify which row
		public void UpdateDatabase(RecoveryRecord record)
		{
			string query = @"UPDATE CarRecoveries 
                             SET CarModel = @model, Issue = @issue, Location = @location,
                                 Status = @status, BreakdownTime = @time
                             WHERE NumberPlate = @plate";

			using SqlConnection conn = new SqlConnection(connectionString);
			SqlCommand cmd = new SqlCommand(query, conn);

			cmd.Parameters.AddWithValue("@plate", record.NumberPlate);
			cmd.Parameters.AddWithValue("@model", record.CarModel);
			cmd.Parameters.AddWithValue("@issue", record.Issue);
			cmd.Parameters.AddWithValue("@location", record.Location);
			cmd.Parameters.AddWithValue("@status", record.Status);
			cmd.Parameters.AddWithValue("@time", record.BreakdownTime);

			conn.Open();
			cmd.ExecuteNonQuery();
		}

		// Deletes a record from the database using the number plate as the key
		public void DeleteFromDatabase(string numberPlate)
		{
			string query = "DELETE FROM CarRecoveries WHERE NumberPlate = @plate";

			using SqlConnection conn = new SqlConnection(connectionString);
			SqlCommand cmd = new SqlCommand(query, conn);
			cmd.Parameters.AddWithValue("@plate", numberPlate);

			conn.Open();
			cmd.ExecuteNonQuery();
		}


		// --- All records (used by the All Breakdowns tab) --------------------

		// Returns all records from the database with optional sorting or status filtering.
		// sortBy can be "date" or "status".
		// When sortBy is "date", order is "asc" or "desc".
		// When sortBy is "status", order is the specific status to filter by (e.g. "Pending").
		public List<RecoveryRecord> GetAllRecords(string sortBy = "date", string order = "desc")
		{
			string query;

			if (sortBy == "status")
			{
				// Filter to only show records matching the chosen status
				query = "SELECT NumberPlate, CarModel, Issue, Location, BreakdownTime, Status FROM CarRecoveries WHERE Status = @status ORDER BY BreakdownTime DESC";
			}
			else
			{
				// Sort by date, newest or oldest first
				string direction = order == "asc" ? "ASC" : "DESC";
				query = $"SELECT NumberPlate, CarModel, Issue, Location, BreakdownTime, Status FROM CarRecoveries ORDER BY BreakdownTime {direction}";
			}

			var records = new List<RecoveryRecord>();

			using SqlConnection conn = new SqlConnection(connectionString);
			using SqlCommand cmd = new SqlCommand(query, conn);

			if (sortBy == "status")
				cmd.Parameters.AddWithValue("@status", order); // order holds the status value here

			conn.Open();

			using SqlDataReader reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				records.Add(new RecoveryRecord(
					reader["NumberPlate"].ToString(),
					reader["CarModel"].ToString(),
					reader["Issue"].ToString(),
					reader["Location"].ToString(),
					(DateTime)reader["BreakdownTime"],
					reader["Status"].ToString()
				));
			}

			return records;
		}


		// --- Dashboard stats ------------------------------------------------

		// Gets all the numbers shown on the dashboard - today's count, this month,
		// total, status breakdown, and the per-day data for the bar chart.
		public BreakdownStats GetBreakdownStats()
		{
			var stats = new BreakdownStats();

			using SqlConnection conn = new SqlConnection(connectionString);
			conn.Open();

			// Get all the summary counts in one query rather than running five separate ones
			string summaryQuery = @"
                SELECT
                    COUNT(*) AS Total,
                    SUM(CASE WHEN CAST(BreakdownTime AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS Today,
                    SUM(CASE WHEN YEAR(BreakdownTime)  = YEAR(GETDATE())
                              AND MONTH(BreakdownTime) = MONTH(GETDATE()) THEN 1 ELSE 0 END) AS ThisMonth,
                    SUM(CASE WHEN Status = 'Pending'     THEN 1 ELSE 0 END) AS Pending,
                    SUM(CASE WHEN Status = 'In Progress' THEN 1 ELSE 0 END) AS InProgress,
                    SUM(CASE WHEN Status = 'Completed'   THEN 1 ELSE 0 END) AS Completed
                FROM CarRecoveries";

			using (SqlCommand cmd = new SqlCommand(summaryQuery, conn))
			using (SqlDataReader reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					stats.TotalCount = (int)reader["Total"];
					stats.TodayCount = (int)reader["Today"];
					stats.ThisMonthCount = (int)reader["ThisMonth"];
					stats.PendingCount = (int)reader["Pending"];
					stats.InProgressCount = (int)reader["InProgress"];
					stats.CompletedCount = (int)reader["Completed"];
				}
			}

			// Get daily breakdown counts for the last 30 days for the bar chart
			string dailyQuery = @"
                SELECT
                    CAST(BreakdownTime AS DATE) AS Day,
                    COUNT(*) AS DayCount
                FROM CarRecoveries
                WHERE BreakdownTime >= DATEADD(DAY, -29, CAST(GETDATE() AS DATE))
                GROUP BY CAST(BreakdownTime AS DATE)
                ORDER BY Day";

			using (SqlCommand cmd = new SqlCommand(dailyQuery, conn))
			using (SqlDataReader reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					stats.Last30Days.Add(new DailyCount
					{
						Date = ((DateTime)reader["Day"]).ToString("dd MMM"),
						Count = (int)reader["DayCount"]
					});
				}
			}

			return stats;
		}


		// --- User / auth operations -----------------------------------------

		// Looks up a user by username - returns null if they don't exist
		public AuthRecord GetUserByUsername(string username)
		{
			string query = "SELECT Id, Username, PasswordHash, FullName, Role, CreatedAt FROM Users WHERE Username = @username";

			using SqlConnection conn = new SqlConnection(connectionString);
			SqlCommand cmd = new SqlCommand(query, conn);
			cmd.Parameters.AddWithValue("@username", username);

			conn.Open();
			SqlDataReader reader = cmd.ExecuteReader();

			if (reader.Read())
			{
				return new AuthRecord
				{
					Id = (int)reader["Id"],
					Username = reader["Username"].ToString(),
					PasswordHash = reader["PasswordHash"].ToString(),
					FullName = reader["FullName"].ToString(),
					Role = reader["Role"].ToString(),
					CreatedAt = (DateTime)reader["CreatedAt"]
				};
			}

			return null;
		}

		// Quick check to see if a username is already taken before registering
		public bool UsernameExists(string username)
		{
			using SqlConnection conn = new SqlConnection(connectionString);
			SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Username = @username", conn);
			cmd.Parameters.AddWithValue("@username", username);
			conn.Open();
			return (int)cmd.ExecuteScalar() > 0;
		}

		// Inserts a new user into the database
		public void CreateUser(AuthRecord user)
		{
			string query = @"INSERT INTO Users (Username, PasswordHash, FullName, Role, CreatedAt) 
                             VALUES (@username, @hash, @fullname, @role, @created)";

			using SqlConnection conn = new SqlConnection(connectionString);
			SqlCommand cmd = new SqlCommand(query, conn);

			cmd.Parameters.AddWithValue("@username", user.Username);
			cmd.Parameters.AddWithValue("@hash", user.PasswordHash);
			cmd.Parameters.AddWithValue("@fullname", user.FullName);
			cmd.Parameters.AddWithValue("@role", user.Role);
			cmd.Parameters.AddWithValue("@created", user.CreatedAt);

			conn.Open();
			cmd.ExecuteNonQuery();
		}


		// --- Password hashing -----------------------------------------------

		// Hashes a password using SHA-256 with a salt before storing it.
		// I know bcrypt would be more secure but this is fine for a coursework project.
		public static string HashPassword(string password)
		{
			using var sha = SHA256.Create();
			byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + "CST2550_SALT"));
			return Convert.ToBase64String(bytes);
		}

		public static bool VerifyPassword(string password, string storedHash)
		{
			return HashPassword(password) == storedHash;
		}
	}
}
