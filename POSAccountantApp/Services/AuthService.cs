using System;
using System.Data;
using System.Data.SqlClient;
using POSAccountantApp.Models;
using POSAccountantApp.Database;

namespace POSAccountantApp.Services
{
    public class AuthService
    {
        public User Login(string username, string password)
        {
            try
            {
                string query = @"
                    SELECT UserId, Username, PasswordHash, Role, FullName, CreatedDate, IsActive 
                    FROM Users 
                    WHERE Username = @Username AND IsActive = 1";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", username)
                };

                DataTable result = SqlConnectionHelper.ExecuteQuery(query, parameters);

                if (result.Rows.Count == 0)
                {
                    throw new Exception("User not found.");
                }

                DataRow userRow = result.Rows[0];
                var user = new User
                {
                    UserId = Convert.ToInt32(userRow["UserId"]),
                    Username = userRow["Username"].ToString(),
                    PasswordHash = userRow["PasswordHash"].ToString(),
                    Role = (UserRole)Enum.Parse(typeof(UserRole), userRow["Role"].ToString()),
                    FullName = userRow["FullName"].ToString(),
                    CreatedDate = Convert.ToDateTime(userRow["CreatedDate"]),
                    IsActive = Convert.ToBoolean(userRow["IsActive"])
                };

                if (!user.VerifyPassword(password))
                {
                    throw new Exception("Invalid password.");
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}", ex);
            }
        }

        public void CreateUser(User user, string password)
        {
            try
            {
                string query = @"
                    INSERT INTO Users (Username, PasswordHash, Role, FullName, CreatedDate, IsActive)
                    VALUES (@Username, @PasswordHash, @Role, @FullName, @CreatedDate, @IsActive)";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Username", user.Username),
                    new SqlParameter("@PasswordHash", User.HashPassword(password)),
                    new SqlParameter("@Role", user.Role.ToString()),
                    new SqlParameter("@FullName", user.FullName),
                    new SqlParameter("@CreatedDate", user.CreatedDate),
                    new SqlParameter("@IsActive", user.IsActive)
                };

                SqlConnectionHelper.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create user: {ex.Message}", ex);
            }
        }

        public void UpdateUser(User user)
        {
            try
            {
                string query = @"
                    UPDATE Users 
                    SET Role = @Role,
                        FullName = @FullName,
                        IsActive = @IsActive
                    WHERE UserId = @UserId";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", user.UserId),
                    new SqlParameter("@Role", user.Role.ToString()),
                    new SqlParameter("@FullName", user.FullName),
                    new SqlParameter("@IsActive", user.IsActive)
                };

                SqlConnectionHelper.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user: {ex.Message}", ex);
            }
        }

        public void ChangePassword(int userId, string newPassword)
        {
            try
            {
                string query = @"
                    UPDATE Users 
                    SET PasswordHash = @PasswordHash
                    WHERE UserId = @UserId";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@PasswordHash", User.HashPassword(newPassword))
                };

                SqlConnectionHelper.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to change password: {ex.Message}", ex);
            }
        }
    }
}
