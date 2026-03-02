using Application.Interfaces;
using Domain.Entities;
using Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infastructure.Repositories
{
    /// <summary>
    /// Repository for OrgUser entity operations.
    /// Provides data access methods for user-organization relationships.
    /// </summary>
    public class OrgUserRepository : BaseRepository<OrgUser>, IOrgUserRepository
    {
        /// <summary>
        /// Initializes a new instance of the OrgUserRepository class.
        /// </summary>
        /// <param name="context">The application DbContext.</param>
        public OrgUserRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets the relationship between a user and organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="userId">The user ID.</param>
        public async Task<OrgUser?> GetByOrganizationAndUserAsync(Guid organizationId, Guid userId)
        {
            return await _dbSet
                .Include(ou => ou.Organization)
                .Include(ou => ou.User)
                .FirstOrDefaultAsync(ou =>
                    ou.OrganizationId == organizationId &&
                    ou.UserId == userId &&
                    !ou.IsDeleted);
        }

        /// <summary>
        /// Gets all users in an organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        public async Task<List<OrgUser>> GetUsersByOrganizationAsync(Guid organizationId)
        {
            return await _dbSet
                .Include(ou => ou.User)
                .Where(ou => ou.OrganizationId == organizationId && !ou.IsDeleted)
                .ToListAsync();
        }
    }
}
