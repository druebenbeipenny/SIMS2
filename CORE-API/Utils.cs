using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace CORE_API
{
    internal class Utils
    {

        public static string HashPassword(string password)
        {

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string hash1, string hash2)
        {
            return hash1 == hash2;
        }

        public static string GeneratePassword(int length)
        {
            string ValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();

            if (length < 1)
                throw new ArgumentException("Password length must be greater than 0.");

            char[] password = new char[length];
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(ValidChars.Length);
                password[i] = ValidChars[index];
            }

            return new string(password);
        }

    }
}
