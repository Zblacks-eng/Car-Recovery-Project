using System;
using Microsoft.Data.SqlClient;

namespace CST2550
{
    public class DatabaseManager
    {
        private string connString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CarRecoveryDB;Integrated Security=True;TrustServerCertificate=True";

        public void LoadFromDatabase(RecoveryTree tree)
        {
            using SqlConnection conn = new SqlConnection(connString);
            string sql = "SELECT NumberPlate, CarModel, Status FROM CarRecoveries";
            SqlCommand cmd = new SqlCommand(sql, conn);
            try {
                conn.Open();
                using SqlDataReader r = cmd.ExecuteReader();
                while (r.Read()) {
                    tree.Add(new RecoveryRecord(r["NumberPlate"].ToString()!, r["CarModel"].ToString()!));
                }
            } catch (Exception) { /* Basic error handling for now */ }
        }

        public void SaveToDatabase(RecoveryRecord rec)
        {
            using SqlConnection conn = new SqlConnection(connString);
            string sql = "INSERT INTO CarRecoveries (NumberPlate, CarModel, Status) VALUES (@p, @m, @s)";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@p", rec.NumberPlate);
            cmd.Parameters.AddWithValue("@m", rec.CarModel);
            cmd.Parameters.AddWithValue("@s", "Pending");
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
