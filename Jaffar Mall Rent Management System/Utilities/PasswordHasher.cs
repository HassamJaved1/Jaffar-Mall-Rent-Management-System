namespace Jaffar_Mall_Rent_Management_System.Utilities
{
    using System.Security.Cryptography;

    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            try
            {
                // Generate random salt (16 bytes)
                byte[] salt = RandomNumberGenerator.GetBytes(16);

                // Derive 32-byte key using PBKDF2 SHA-256
                byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,
                    salt: salt,
                    iterations: 100000,
                    hashAlgorithm: HashAlgorithmName.SHA256,
                    outputLength: 32
                );

                // Combine salt + hash
                byte[] result = new byte[salt.Length + hash.Length];
                Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
                Buffer.BlockCopy(hash, 0, result, salt.Length, hash.Length);

                return Convert.ToBase64String(result);
            }
            catch (Exception e) 
            {
                //Log the exception message if needed
                throw new Exception("Error hashing password", e);
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(storedHash);

                // Extract salt (first 16 bytes)
                byte[] salt = new byte[16];
                Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);

                // Extract stored hash
                byte[] storedPasswordHash = new byte[32];
                Buffer.BlockCopy(hashBytes, 16, storedPasswordHash, 0, 32);

                // Recompute hash with same salt
                byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                    password: password,
                    salt: salt,
                    iterations: 100000,
                    hashAlgorithm: HashAlgorithmName.SHA256,
                    outputLength: 32
                );

                return CryptographicOperations.FixedTimeEquals(storedPasswordHash, computedHash);
            }
            catch (Exception e) 
            {
                //Log the exception message if needed
                return false;
            }
        }
    }

}
