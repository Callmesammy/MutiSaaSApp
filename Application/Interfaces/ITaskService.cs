using Application.DTOs.Task;
using Domain.Enums;

namespace Application.Interfaces
{
    /// <summary>
    /// Interface for task management operations.
    /// Handles task CRUD operations scoped to organizations.
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Creates a new task in the specified organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="userId">The ID of the user creating the task.</param>
        /// <param name="request">The create task request.</param>
        /// <returns>The created task response.</returns>
        Task<TaskResponse> CreateTaskAsync(Guid organizationId, Guid userId, CreateTaskRequest request);

        /// <summary>
        /// Retrieves a task by ID.
        /// Verifies the task belongs to the specified organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="taskId">The task ID to retrieve.</param>
        /// <returns>The task response, or null if not found.</returns>
        Task<TaskResponse?> GetTaskAsync(Guid organizationId, Guid taskId);

        /// <summary>
        /// Retrieves all tasks for an organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <returns>A list of all tasks in the organization.</returns>
        Task<List<TaskResponse>> GetTasksAsync(Guid organizationId);

        /// <summary>
        /// Retrieves all tasks for an organization with optional filtering.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="status">Filter by status (optional).</param>
        /// <param name="priority">Filter by priority (optional).</param>
        /// <param name="assigneeId">Filter by assignee ID (optional).</param>
        /// <returns>A list of filtered tasks.</returns>
        Task<List<TaskResponse>> GetTasksFilteredAsync(
            Guid organizationId,
            Domain.Enums.TaskStatus? status = null,
            Domain.Enums.TaskPriority? priority = null,
            Guid? assigneeId = null);

        /// <summary>
        /// Updates an existing task.
        /// Only admins can update any task; members can only update unassigned fields.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="taskId">The task ID to update.</param>
        /// <param name="userId">The ID of the user performing the update.</param>
        /// <param name="userRole">The role of the user performing the update.</param>
        /// <param name="request">The update task request.</param>
        /// <returns>The updated task response.</returns>
        Task<TaskResponse> UpdateTaskAsync(
            Guid organizationId,
            Guid taskId,
            Guid userId,
            Domain.Enums.UserRole userRole,
            UpdateTaskRequest request);

        /// <summary>
        /// Deletes (soft-deletes) a task.
        /// Only admins can delete tasks.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="taskId">The task ID to delete.</param>
        /// <returns>True if the task was successfully deleted.</returns>
        Task<bool> DeleteTaskAsync(Guid organizationId, Guid taskId);

        /// <summary>
        /// Gets the total count of tasks in an organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        Task<int> GetTaskCountAsync(Guid organizationId);
    }
}
