using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.PasswordHashing
{
    public class PasswordHasher
    {
        private readonly Random _random = new Random();
        private const int SaltSize = 16; // Kích thước salt (byte)

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty.");

            // Tạo salt ngẫu nhiên
            byte[] salt = new byte[SaltSize];
            _random.NextBytes(salt);

            // Kết hợp password với salt
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] passwordWithSalt = new byte[passwordBytes.Length + salt.Length];
            Buffer.BlockCopy(passwordBytes, 0, passwordWithSalt, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, passwordWithSalt, passwordBytes.Length, salt.Length);

            // Hash với SHA256
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(passwordWithSalt);

                // Kết hợp hash và salt thành một chuỗi duy nhất (có thể lưu dưới dạng base64)
                byte[] hashWithSalt = new byte[hashBytes.Length + salt.Length];
                Buffer.BlockCopy(hashBytes, 0, hashWithSalt, 0, hashBytes.Length);
                Buffer.BlockCopy(salt, 0, hashWithSalt, hashBytes.Length, salt.Length);

                return Convert.ToBase64String(hashWithSalt);
            }
        }

        public bool VerifyPassword(string passwordRequest, string dbPassword)
        {
            if (string.IsNullOrEmpty(passwordRequest) || string.IsNullOrEmpty(dbPassword))
                return false;

            // Giải mã hashWithSalt từ base64
            byte[] hashWithSalt = Convert.FromBase64String(dbPassword);
            int hashSize = 32; // Kích thước hash của SHA256
            byte[] salt = new byte[SaltSize];
            byte[] storedHash = new byte[hashSize];

            // Tách salt và hash đã lưu
            Buffer.BlockCopy(hashWithSalt, 0, storedHash, 0, hashSize);
            Buffer.BlockCopy(hashWithSalt, hashSize, salt, 0, SaltSize);

            // Tạo hash mới với cùng salt
            byte[] passwordBytes = Encoding.UTF8.GetBytes(passwordRequest);
            byte[] passwordWithSalt = new byte[passwordBytes.Length + salt.Length];
            Buffer.BlockCopy(passwordBytes, 0, passwordWithSalt, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, passwordWithSalt, passwordBytes.Length, salt.Length);

            using (var sha256 = SHA256.Create())
            {
                byte[] newHash = sha256.ComputeHash(passwordWithSalt);
                return StructuralComparisons.StructuralEqualityComparer.Equals(newHash, storedHash);
            }
        }
    }
}
