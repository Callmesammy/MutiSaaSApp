namespace Application.DTOs.Invite
{
    /// <summary>
    /// Request DTO for creating an invite token.
    /// Only admins can send invites.
    /// </summary>
    public class CreateInviteRequest
    {
        /// <summary>
        /// Gets or sets the email address to invite.
        /// Must be a valid email format.
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for accepting an invite.
    /// User provides the token and creates their account.
    /// </summary>
    public class AcceptInviteRequest
    {
        /// <summary>
        /// Gets or sets the invite token sent to the user's email.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for the new user account.
        /// Must meet minimum strength requirements.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name of the new user.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the new user.
        /// </summary>
        public string? LastName { get; set; }
    }

    /// <summary>
    /// Response DTO for a successful invite.
    /// Contains the invite token information.
    /// </summary>
    public class InviteResponse
    {
        /// <summary>
        /// Gets or sets the unique ID of the invite token.
        /// </summary>
        public Guid InviteTokenId { get; set; }

        /// <summary>
        /// Gets or sets the email that was invited.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the organization ID.
        /// </summary>
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the invite token that should be shared with the user.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the token expires.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the message for the invitee.
        /// </summary>
        public string Message { get; set; } = "You have been invited to join an organization. Use this token to accept the invitation.";
    }
}
