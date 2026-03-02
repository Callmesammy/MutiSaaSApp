using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace Infastructure.Services
{
    /// <summary>
    /// Service for generating and validating JWT tokens.
    /// Handles token creation with claims for authentication and authorization.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a JWT token for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="email">The user email.</param>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="role">The user role within the organization.</param>
        string GenerateToken(Guid userId, string email, Guid organizationId, UserRole role);
    }

    /// <summary>
    /// Implementation of JWT token generation service.
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly string _secretKey;
        private readonly int _expiryMinutes;

        /// <summary>
        /// Initializes a new instance of the JwtTokenService class.
        /// </summary>
        /// <param name="secretKey">The secret key for signing tokens.</param>
        /// <param name="expiryMinutes">Token expiry time in minutes (default: 60).</param>
        public JwtTokenService(string secretKey, int expiryMinutes = 60)
        {
            _secretKey = secretKey;
            _expiryMinutes = expiryMinutes;
        }

        /// <summary>
        /// Generates a JWT token with the provided user information.
        /// Token includes claims for user ID, email, organization ID, and role.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="email">The user email.</param>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="role">The user role.</param>
        public string GenerateToken(Guid userId, string email, Guid organizationId, UserRole role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim("org_id", organizationId.ToString()),
                new Claim(ClaimTypes.Role, role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "TeamFlow",
                audience: "TeamFlow",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
