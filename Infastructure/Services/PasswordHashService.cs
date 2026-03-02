using BCrypt.Net;

namespace Infastructure.Services
{
    /// <summary>
    /// Service for password hashing and verification.
    /// Uses BCrypt for secure password storage.
    /// </summary>
    public interface IPasswordHashService
    {
        /// <summary>
        /// Hashes a plain text password using BCrypt.
        /// </summary>
        /// <param name="password">The plain text password.</param>
        string HashPassword(string password);

        /// <summary>
        /// Verifies a plain text password against a hash.
        /// </summary>
        /// <param name="password">The plain text password.</param>
        /// <param name="hash">The password hash to verify against.</param>
        bool VerifyPassword(string password, string hash);
    }

    /// <summary>
    /// Implementation of password hashing service using BCrypt.
    /// </summary>
    public class PasswordHashService : IPasswordHashService
    {
        /// <summary>
        /// Hashes a password using BCrypt with default work factor.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifies a password matches a BCrypt hash.
        /// </summary>
        /// <param name="password">The plain text password to verify.</param>
        /// <param name="hash">The BCrypt hash to verify against.</param>
        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
