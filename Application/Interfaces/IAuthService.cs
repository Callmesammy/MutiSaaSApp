using Application.DTOs.Auth;

namespace Application.Interfaces
{
    /// <summary>
    /// Interface for authentication-related operations.
    /// Handles user registration, login, and token management.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new organization and creates an admin user.
        /// The requesting user is automatically assigned the Admin role.
        /// </summary>
        /// <param name="request">The registration request containing organization name, email, and password.</param>
        /// <returns>An AuthResponse containing user info and JWT token.</returns>
        Task<AuthResponse> RegisterOrganizationAsync(RegisterOrganizationRequest request);

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="organizationId">The organization ID for multi-tenant context.</param>
        /// <returns>An AuthResponse containing user info and JWT token.</returns>
        Task<AuthResponse> LoginAsync(string email, string password, Guid organizationId);
    }
}
