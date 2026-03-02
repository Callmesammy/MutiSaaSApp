using Application.Interfaces;
using Domain.Entities;
using Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infastructure.Repositories
{
    /// <summary>
    /// Repository for User entity operations.
    /// Provides data access methods specific to users.
    /// </summary>
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        /// <summary>
        /// Initializes a new instance of the UserRepository class.
        /// </summary>
        /// <param name="context">The application DbContext.</param>
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets a user by email address.
        /// </summary>
        /// <param name="email">The email to search for.</param>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        /// <summary>
        /// Checks if a user with the given email exists.
        /// </summary>
        /// <param name="email">The email to check.</param>
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email && !u.IsDeleted);
        }
    }
}
