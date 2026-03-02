using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// Represents an invitation token sent to a user to join an organization.
    /// Tokens are single-use and expire after 48 hours.
    /// </summary>
    public class InviteToken : BaseEntity
    {
        /// <summary>
        /// Gets or sets the token string.
        /// Should be cryptographically secure and unique.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address the invite is sent to.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the organization the user is being invited to.
        /// </summary>
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who sent the invite (Admin).
        /// Null if the invite was sent by system.
        /// </summary>
        public Guid? InvitedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the token expires.
        /// Typically 48 hours from creation.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the token has been used.
        /// Once used (accepted), the token cannot be reused.
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the invite was accepted.
        /// Null if not yet accepted.
        /// </summary>
        public DateTime? AcceptedAt { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who accepted the invite.
        /// Null if not yet accepted.
        /// </summary>
        public Guid? AcceptedByUserId { get; set; }

        /// <summary>
        /// Navigation property: the organization.
        /// </summary>
        public Organization? Organization { get; set; }

        /// <summary>
        /// Navigation property: the user who sent the invite.
        /// </summary>
        public User? InvitedByUser { get; set; }

        /// <summary>
        /// Navigation property: the user who accepted the invite.
        /// </summary>
        public User? AcceptedByUser { get; set; }

        /// <summary>
        /// Initializes a new instance of the InviteToken class.
        /// </summary>
        public InviteToken() : base()
        {
            IsUsed = false;
        }

        /// <summary>
        /// Creates a new invite token with the specified parameters.
        /// </summary>
        /// <param name="token">The unique token string.</param>
        /// <param name="email">The email address being invited.</param>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="invitedByUserId">The user ID of the person sending the invite.</param>
        /// <param name="expiryHours">Hours until the token expires (default: 48).</param>
        public InviteToken(
            string token,
            string email,
            Guid organizationId,
            Guid invitedByUserId,
            int expiryHours = 48) : base()
        {
            Token = token;
            Email = email;
            OrganizationId = organizationId;
            InvitedByUserId = invitedByUserId;
            ExpiresAt = DateTime.UtcNow.AddHours(expiryHours);
            IsUsed = false;
        }

        /// <summary>
        /// Checks if the token is still valid (not expired and not used).
        /// </summary>
        public bool IsValid()
        {
            return !IsUsed && DateTime.UtcNow < ExpiresAt;
        }

        /// <summary>
        /// Checks if the token has expired.
        /// </summary>
        public bool IsExpired()
        {
            return DateTime.UtcNow >= ExpiresAt;
        }

        /// <summary>
        /// Marks the token as used by accepting the invite.
        /// </summary>
        /// <param name="userId">The ID of the user accepting the invite.</param>
        public void Accept(Guid userId)
        {
            IsUsed = true;
            AcceptedAt = DateTime.UtcNow;
            AcceptedByUserId = userId;
        }
    }
}
