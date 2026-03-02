using Application.Interfaces;
using Domain.Common;
using Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infastructure.Repositories
{
    /// <summary>
    /// Generic repository base class providing common CRUD operations.
    /// Handles soft delete operations and context management.
    /// </summary>
    /// <typeparam name="T">The entity type inheriting from BaseEntity.</typeparam>
    public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// The DbContext for database operations.
        /// </summary>
        protected readonly ApplicationDbContext _context;

        /// <summary>
        /// The DbSet for the entity type.
        /// </summary>
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the BaseRepository class.
        /// </summary>
        /// <param name="context">The application DbContext.</param>
        protected BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Gets all entities without soft delete filter applied.
        /// </summary>
        public IQueryable<T> GetAll()
        {
            return _dbSet.AsNoTracking();
        }

        /// <summary>
        /// Gets all non-deleted (active) entities.
        /// </summary>
        public IQueryable<T> GetAllActive()
        {
            return _dbSet.Where(e => !e.IsDeleted).AsNoTracking();
        }

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="id">The entity ID.</param>
        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public async Task UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Soft deletes an entity by marking IsDeleted as true.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public async Task DeleteAsync(T entity)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Saves all changes to the database.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
