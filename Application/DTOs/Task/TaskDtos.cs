using Domain.Enums;

namespace Application.DTOs.Task
{
    /// <summary>
    /// Request DTO for creating a new task.
    /// </summary>
    public class CreateTaskRequest
    {
        /// <summary>
        /// Gets or sets the title of the task.
        /// Required and must be non-empty.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the task.
        /// Optional field.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the priority level.
        /// Default is Medium.
        /// </summary>
        public Domain.Enums.TaskPriority Priority { get; set; } = Domain.Enums.TaskPriority.Medium;

        /// <summary>
        /// Gets or sets the ID of the user to assign the task to.
        /// Optional - task can be created without assignment.
        /// </summary>
        public Guid? AssigneeId { get; set; }

        /// <summary>
        /// Gets or sets the due date for the task.
        /// Optional field.
        /// </summary>
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// Request DTO for updating an existing task.
    /// </summary>
    public class UpdateTaskRequest
    {
        /// <summary>
        /// Gets or sets the updated title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the updated description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the updated status.
        /// </summary>
        public Domain.Enums.TaskStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the updated priority level.
        /// </summary>
        public Domain.Enums.TaskPriority? Priority { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user to assign to.
        /// Set to Guid.Empty to unassign.
        /// </summary>
        public Guid? AssigneeId { get; set; }

        /// <summary>
        /// Gets or sets the updated due date.
        /// </summary>
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// Response DTO for task data.
    /// Used in GET endpoints and after create/update operations.
    /// </summary>
    public class TaskResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier of the task.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the task.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the task.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the current status of the task.
        /// </summary>
        public Domain.Enums.TaskStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the priority level of the task.
        /// </summary>
        public Domain.Enums.TaskPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the organization ID.
        /// </summary>
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created the task.
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the email of the user who created the task.
        /// </summary>
        public string CreatedByEmail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the user assigned to this task.
        /// </summary>
        public Guid? AssigneeId { get; set; }

        /// <summary>
        /// Gets or sets the email of the assigned user.
        /// </summary>
        public string? AssigneeEmail { get; set; }

        /// <summary>
        /// Gets or sets the due date for the task.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
