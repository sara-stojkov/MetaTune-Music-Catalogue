using System.Security.Cryptography;

namespace Core.Utils
{
    public static class PasswordManager
    {

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            // Derive the hash
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32); // 256-bit hash
                // Store as base64(salt):base64(hash)
                return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
            }
        }

        public static bool ArePasswordsEqual(string password, string hashedPassword)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            // Check if hashedPassword has correct format (salt:hash)
            if (!hashedPassword.Contains(":"))
                return false;

            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return false;

            try
            {
                // Extract salt and stored hash
                var salt = Convert.FromBase64String(parts[0]);
                var storedHash = parts[1];

                // Hash the input password with the same salt
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256))
                {
                    byte[] hash = pbkdf2.GetBytes(32); // 256-bit hash
                    var computedHash = Convert.ToBase64String(hash);

                    // Compare the computed hash with stored hash
                    return computedHash == storedHash;
                }
            }
            catch (FormatException)
            {
                // Invalid base64 format
                return false;
            }
            catch (ArgumentException)
            {
                // Invalid arguments for PBKDF2
                return false;
            }
        }
    }
}
