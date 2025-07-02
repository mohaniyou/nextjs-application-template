using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace POSAccountantApp.Database
{
    public static class SqlConnectionHelper
    {
        // TODO: Move to configuration file
        private static readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=POSAccountingDB;Integrated Security=True;Connect Timeout=30;";

        public static SqlConnection GetConnection()
        {
            try
            {
                var connection = new SqlConnection(connectionString);
                connection.Open();
                return connection;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database connection error: {ex.Message}", ex);
            }
        }

        public static void ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw new Exception($"Database query error: {ex.Message}", ex);
                }
            }
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                try
                {
                    var dataTable = new DataTable();
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                    return dataTable;
                }
                catch (SqlException ex)
                {
                    throw new Exception($"Database query error: {ex.Message}", ex);
                }
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                try
                {
                    return command.ExecuteScalar();
                }
                catch (SqlException ex)
                {
                    throw new Exception($"Database query error: {ex.Message}", ex);
                }
            }
        }

        public static void InitializeDatabase()
        {
            // Create database if it doesn't exist
            string createDbQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'POSAccountingDB')
                BEGIN
                    CREATE DATABASE POSAccountingDB;
                END";

            // Create tables
            string createTablesQuery = @"
                USE POSAccountingDB;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                BEGIN
                    CREATE TABLE Users (
                        UserId INT PRIMARY KEY IDENTITY(1,1),
                        Username NVARCHAR(50) UNIQUE NOT NULL,
                        PasswordHash NVARCHAR(256) NOT NULL,
                        Role NVARCHAR(20) NOT NULL,
                        FullName NVARCHAR(100) NOT NULL,
                        CreatedDate DATETIME NOT NULL,
                        IsActive BIT NOT NULL
                    );

                    -- Insert default admin user
                    INSERT INTO Users (Username, PasswordHash, Role, FullName, CreatedDate, IsActive)
                    VALUES ('admin', '" + Models.User.HashPassword("admin") + @"', 'Admin', 'System Administrator', GETDATE(), 1);
                END";

            try
            {
                // Create database
                using (var connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;"))
                {
                    connection.Open();
                    using (var command = new SqlCommand(createDbQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Create tables
                ExecuteNonQuery(createTablesQuery);
            }
            catch (Exception ex)
            {
                throw new Exception($"Database initialization error: {ex.Message}", ex);
            }
        }
    }
}
