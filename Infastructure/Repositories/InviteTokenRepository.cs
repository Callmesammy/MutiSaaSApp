using Application.Interfaces;
using Domain.Entities;
using Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infastructure.Repositories
{
    /// <summary>
    /// Repository for InviteToken entity operations.
    /// Provides data access methods for managing invitation tokens.
    /// </summary>
    public class InviteTokenRepository : BaseRepository<InviteToken>, IInviteTokenRepository
    {
        /// <summary>
        /// Initializes a new instance of the InviteTokenRepository class.
        /// </summary>
        /// <param name="context">The application DbContext.</param>
        public InviteTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets an invite token by its token string.
        /// </summary>
        /// <param name="token">The token string to search for.</param>
        public async Task<InviteToken?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(it => it.Organization)
                .Include(it => it.InvitedByUser)
                .FirstOrDefaultAsync(it => it.Token == token && !it.IsDeleted);
        }

        /// <summary>
        /// Gets all unexpired, unused invites for an email address.
        /// Includes the organization and inviting user details.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        public async Task<List<InviteToken>> GetValidInvitesByEmailAsync(string email)
        {
            return await _dbSet
                .Include(it => it.Organization)
                .Include(it => it.InvitedByUser)
                .Where(it =>
                    it.Email == email &&
                    !it.IsUsed &&
                    it.ExpiresAt > DateTime.UtcNow &&
                    !it.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all unexpired, unused invites for an organization.
        /// Useful for listing pending invites sent by an admin.
        /// </summary>
        /// <param name="organizationId">The organization ID to search for.</param>
        public async Task<List<InviteToken>> GetValidInvitesByOrganizationAsync(Guid organizationId)
        {
            return await _dbSet
                .Include(it => it.InvitedByUser)
                .Where(it =>
                    it.OrganizationId == organizationId &&
                    !it.IsUsed &&
                    it.ExpiresAt > DateTime.UtcNow &&
                    !it.IsDeleted)
                .ToListAsync();
        }
    }
}
