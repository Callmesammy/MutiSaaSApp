namespace Application.DTOs.Auth
{
    /// <summary>
    /// Request DTO for registering a new organization and admin user.
    /// </summary>
    public class RegisterOrganizationRequest
    {
        /// <summary>
        /// Gets or sets the name of the organization to be created.
        /// Must be unique and not empty.
        /// </summary>
        public string OrganizationName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email of the admin user.
        /// Must be a valid email and unique across the system.
        /// </summary>
        public string AdminEmail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for the admin user.
        /// Must meet minimum strength requirements.
        /// </summary>
        public string AdminPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO for successful authentication.
    /// Contains user information and JWT token.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the email of the user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the organization the user belongs to.
        /// </summary>
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the organization.
        /// </summary>
        public string OrganizationName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the JWT bearer token for API authentication.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role of the user within the organization.
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }
}
