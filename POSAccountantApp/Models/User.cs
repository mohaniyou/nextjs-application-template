using System;
using System.Security.Cryptography;
using System.Text;

namespace POSAccountantApp.Models
{
    public enum UserRole
    {
        Admin,
        Cashier,
        Accountant
    }

    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        public User()
        {
            CreatedDate = DateTime.Now;
            IsActive = true;
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string password)
        {
            string hashedInput = HashPassword(password);
            return PasswordHash == hashedInput;
        }

        public override string ToString()
        {
            return $"{Username} ({Role})";
        }
    }
}
