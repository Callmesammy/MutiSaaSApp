using System.Security.Cryptography;
using System.Text;

namespace Infastructure.Services
{
    /// <summary>
    /// Service for generating cryptographically secure tokens.
    /// Used for invite tokens and other temporary access tokens.
    /// </summary>
    public interface ITokenGeneratorService
    {
        /// <summary>
        /// Generates a cryptographically secure random token.
        /// </summary>
        /// <param name="length">Length of the token in bytes (default: 32).</param>
        /// <returns>A base64-encoded secure token string.</returns>
        string GenerateSecureToken(int length = 32);
    }

    /// <summary>
    /// Implementation of token generator service.
    /// </summary>
    public class TokenGeneratorService : ITokenGeneratorService
    {
        /// <summary>
        /// Generates a cryptographically secure random token as a base64 string.
        /// </summary>
        /// <param name="length">The length of the random bytes to generate (default: 32).</param>
        public string GenerateSecureToken(int length = 32)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var tokenBuffer = new byte[length];
                rng.GetBytes(tokenBuffer);
                return Convert.ToBase64String(tokenBuffer);
            }
        }
    }
}
