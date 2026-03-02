using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a user in the system.
    /// Users can belong to multiple organizations with different roles.
    /// </summary>
    public class User : BaseEntity
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// Must be unique across the system.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hashed password of the user.
        /// Never stored in plain text.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Navigation property: collection of organizations the user belongs to.
        /// </summary>
        public ICollection<OrgUser> OrgUsers { get; set; } = new List<OrgUser>();

        /// <summary>
        /// Initializes a new instance of the User class.
        /// </summary>
        public User() : base()
        {
        }

        /// <summary>
        /// Creates a new user with the specified email and password hash.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="passwordHash">The hashed password.</param>
        public User(string email, string passwordHash) : base()
        {
            Email = email;
            PasswordHash = passwordHash;
        }
    }
}
