using Domain.Entities;

namespace Application.Interfaces
{
    /// <summary>
    /// Generic repository interface for data access operations.
    /// Provides base CRUD functionality for all entities.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets all entities without soft delete filter.
        /// </summary>
        IQueryable<T> GetAll();

        /// <summary>
        /// Gets all non-deleted entities.
        /// </summary>
        IQueryable<T> GetAllActive();

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity (soft delete - marks IsDeleted as true).
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Saves all changes to the database.
        /// </summary>
        Task SaveChangesAsync();
    }

    /// <summary>
    /// Repository interface for User entity operations.
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Gets a user by email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Checks if a user with the given email exists.
        /// </summary>
        /// <param name="email">The email to check.</param>
        Task<bool> EmailExistsAsync(string email);
    }

    /// <summary>
    /// Repository interface for Organization entity operations.
    /// </summary>
    public interface IOrganizationRepository : IRepository<Organization>
    {
        /// <summary>
        /// Gets an organization by name.
        /// </summary>
        /// <param name="name">The name of the organization.</param>
        Task<Organization?> GetByNameAsync(string name);

        /// <summary>
        /// Checks if an organization with the given name exists.
        /// </summary>
        /// <param name="name">The name to check.</param>
        Task<bool> NameExistsAsync(string name);
    }

    /// <summary>
    /// Repository interface for OrgUser entity operations.
    /// </summary>
    public interface IOrgUserRepository : IRepository<OrgUser>
    {
        /// <summary>
        /// Gets the relationship between a user and organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="userId">The user ID.</param>
        Task<OrgUser?> GetByOrganizationAndUserAsync(Guid organizationId, Guid userId);

        /// <summary>
        /// Gets all users in an organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        Task<List<OrgUser>> GetUsersByOrganizationAsync(Guid organizationId);
    }

    /// <summary>
    /// Repository interface for InviteToken entity operations.
    /// </summary>
    public interface IInviteTokenRepository : IRepository<InviteToken>
    {
        /// <summary>
        /// Gets an invite token by its token string.
        /// </summary>
        /// <param name="token">The token string to search for.</param>
        Task<InviteToken?> GetByTokenAsync(string token);

        /// <summary>
        /// Gets all unexpired, unused invites for an email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        Task<List<InviteToken>> GetValidInvitesByEmailAsync(string email);

        /// <summary>
        /// Gets all unexpired, unused invites for an organization.
        /// </summary>
        /// <param name="organizationId">The organization ID to search for.</param>
        Task<List<InviteToken>> GetValidInvitesByOrganizationAsync(Guid organizationId);
    }

    /// <summary>
    /// Repository interface for TaskItem entity operations.
    /// </summary>
    public interface ITaskRepository : IRepository<TaskItem>
    {
        /// <summary>
        /// Gets all tasks for an organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        Task<List<TaskItem>> GetTasksByOrganizationAsync(Guid organizationId);

        /// <summary>
        /// Gets tasks for an organization with filtering by status and priority.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="status">Filter by status (optional).</param>
        /// <param name="priority">Filter by priority (optional).</param>
        Task<List<TaskItem>> GetTasksFilteredAsync(
            Guid organizationId,
            Domain.Enums.TaskStatus? status = null,
            Domain.Enums.TaskPriority? priority = null);

        /// <summary>
        /// Gets tasks assigned to a specific user in an organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="assigneeId">The assignee user ID.</param>
        Task<List<TaskItem>> GetTasksByAssigneeAsync(Guid organizationId, Guid assigneeId);

        /// <summary>
        /// Gets the count of tasks in an organization by status.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        Task<int> GetTaskCountByOrganizationAsync(Guid organizationId);
    }
}
