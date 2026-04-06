using System;
using Microsoft.Data.SqlClient; 

namespace CST2550
{
	public class DatabaseManager
	{
		private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CarRecoveryDB;Integrated Security=True;TrustServerCertificate=True";

		public void CheckConnection()
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				try
				{
					conn.Open();
					Console.WriteLine("SQL Connection: SUCCESS. Database is ready.");
				}
				catch (Exception ex)
				{
					Console.WriteLine("SQL Connection: FAILED. " + ex.Message);
				}
			}
		}
	}
}
