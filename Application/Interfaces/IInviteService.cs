using Application.DTOs.Auth;
using Application.DTOs.Invite;

namespace Application.Interfaces
{
    /// <summary>
    /// Interface for invite-related operations.
    /// Handles creating and accepting organization invites.
    /// </summary>
    public interface IInviteService
    {
        /// <summary>
        /// Creates and sends an invite to a user to join an organization.
        /// Only admins can create invites.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="adminUserId">The ID of the admin sending the invite.</param>
        /// <param name="request">The invite request containing the email to invite.</param>
        /// <returns>An InviteResponse containing the token and expiry information.</returns>
        Task<InviteResponse> CreateInviteAsync(Guid organizationId, Guid adminUserId, CreateInviteRequest request);

        /// <summary>
        /// Accepts an invite token and creates a user account.
        /// The user joins the organization as a Member.
        /// </summary>
        /// <param name="request">The accept invite request with token and password.</param>
        /// <returns>An AuthResponse containing user info and JWT token.</returns>
        Task<AuthResponse> AcceptInviteAsync(AcceptInviteRequest request);

        /// <summary>
        /// Retrieves an invite token by its token string.
        /// </summary>
        /// <param name="token">The token string to look up.</param>
        Task<Domain.Entities.InviteToken?> GetInviteTokenAsync(string token);

        /// <summary>
        /// Checks if an invite token is still valid (not expired and not used).
        /// </summary>
        /// <param name="token">The token string to validate.</param>
        Task<bool> IsTokenValidAsync(string token);
    }
}
