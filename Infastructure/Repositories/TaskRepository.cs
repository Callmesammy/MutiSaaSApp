using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infastructure.Repositories
{
    /// <summary>
    /// Repository for TaskItem entity operations.
    /// Provides data access methods for managing tasks scoped to organizations.
    /// </summary>
    public class TaskRepository : BaseRepository<TaskItem>, ITaskRepository
    {
        /// <summary>
        /// Initializes a new instance of the TaskRepository class.
        /// </summary>
        /// <param name="context">The application DbContext.</param>
        public TaskRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets all tasks for an organization.
        /// Tasks are loaded with their creator and assignee information.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        public async Task<List<TaskItem>> GetTasksByOrganizationAsync(Guid organizationId)
        {
            return await _dbSet
                .Include(t => t.CreatedByUser)
                .Include(t => t.Assignee)
                .Where(t => t.OrganizationId == organizationId && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets tasks for an organization with optional filtering by status and priority.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="status">Optional status filter.</param>
        /// <param name="priority">Optional priority filter.</param>
        public async Task<List<TaskItem>> GetTasksFilteredAsync(
            Guid organizationId,
            TaskStatus? status = null,
            TaskPriority? priority = null)
        {
            var query = _dbSet
                .Include(t => t.CreatedByUser)
                .Include(t => t.Assignee)
                .Where(t => t.OrganizationId == organizationId && !t.IsDeleted);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets tasks assigned to a specific user in an organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="assigneeId">The assignee user ID.</param>
        public async Task<List<TaskItem>> GetTasksByAssigneeAsync(Guid organizationId, Guid assigneeId)
        {
            return await _dbSet
                .Include(t => t.CreatedByUser)
                .Include(t => t.Assignee)
                .Where(t =>
                    t.OrganizationId == organizationId &&
                    t.AssigneeId == assigneeId &&
                    !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets the total count of active tasks in an organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        public async Task<int> GetTaskCountByOrganizationAsync(Guid organizationId)
        {
            return await _dbSet
                .Where(t => t.OrganizationId == organizationId && !t.IsDeleted)
                .CountAsync();
        }
    }
}
