using System.Security.Cryptography;

namespace CirendsAPI.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hash password using PBKDF2 with SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltBytes = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                }

                var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
                var hashBytes = pbkdf2.GetBytes(20);

                var hashWithSalt = new byte[36];
                Array.Copy(saltBytes, 0, hashWithSalt, 0, 16);
                Array.Copy(hashBytes, 0, hashWithSalt, 16, 20);

                return Convert.ToBase64String(hashWithSalt);
            }
        }

        /// <summary>
        /// Verify password against hash
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            var hashBytes = Convert.FromBase64String(hash);
            var saltBytes = new byte[16];
            Array.Copy(hashBytes, 0, saltBytes, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256);
            var hashBytes2 = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hashBytes2[i])
                    return false;
            }

            return true;
        }
    }
}
