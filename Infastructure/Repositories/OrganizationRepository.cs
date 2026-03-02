using Application.Interfaces;
using Domain.Entities;
using Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infastructure.Repositories
{
    /// <summary>
    /// Repository for Organization entity operations.
    /// Provides data access methods specific to organizations.
    /// </summary>
    public class OrganizationRepository : BaseRepository<Organization>, IOrganizationRepository
    {
        /// <summary>
        /// Initializes a new instance of the OrganizationRepository class.
        /// </summary>
        /// <param name="context">The application DbContext.</param>
        public OrganizationRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets an organization by name.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        public async Task<Organization?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(o => o.Name == name && !o.IsDeleted);
        }

        /// <summary>
        /// Checks if an organization with the given name exists.
        /// </summary>
        /// <param name="name">The name to check.</param>
        public async Task<bool> NameExistsAsync(string name)
        {
            return await _dbSet.AnyAsync(o => o.Name == name && !o.IsDeleted);
        }
    }
}
