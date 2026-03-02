using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a task item within an organization.
    /// Tasks are scoped to organizations and can only be viewed/edited by organization members.
    /// </summary>
    public class TaskItem : BaseEntity
    {
        /// <summary>
        /// Gets or sets the title of the task.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed description of the task.
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
        /// Gets or sets the ID of the organization that owns this task.
        /// Tasks are always scoped to an organization.
        /// </summary>
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who created the task.
        /// </summary>
        public Guid CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user assigned to complete this task.
        /// Can be null if no one is assigned.
        /// </summary>
        public Guid? AssigneeId { get; set; }

        /// <summary>
        /// Gets or sets the due date for the task.
        /// Can be null if no due date is set.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Navigation property: the organization that owns this task.
        /// </summary>
        public Organization? Organization { get; set; }

        /// <summary>
        /// Navigation property: the user who created the task.
        /// </summary>
        public User? CreatedByUser { get; set; }

        /// <summary>
        /// Navigation property: the user assigned to this task.
        /// </summary>
        public User? Assignee { get; set; }

        /// <summary>
        /// Initializes a new instance of the TaskItem class.
        /// </summary>
        public TaskItem() : base()
        {
            Status = Domain.Enums.TaskStatus.Todo;
            Priority = Domain.Enums.TaskPriority.Medium;
        }

        /// <summary>
        /// Creates a new task with the specified parameters.
        /// </summary>
        /// <param name="title">The title of the task.</param>
        /// <param name="organizationId">The organization that owns the task.</param>
        /// <param name="createdByUserId">The user creating the task.</param>
        /// <param name="status">The initial status (default: Todo).</param>
        /// <param name="priority">The priority level (default: Medium).</param>
        public TaskItem(
            string title,
            Guid organizationId,
            Guid createdByUserId,
            Domain.Enums.TaskStatus status = Domain.Enums.TaskStatus.Todo,
            Domain.Enums.TaskPriority priority = Domain.Enums.TaskPriority.Medium) : base()
        {
            Title = title;
            OrganizationId = organizationId;
            CreatedByUserId = createdByUserId;
            Status = status;
            Priority = priority;
        }

        /// <summary>
        /// Changes the status of the task.
        /// </summary>
        /// <param name="newStatus">The new status.</param>
        public void ChangeStatus(Domain.Enums.TaskStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Changes the priority of the task.
        /// </summary>
        /// <param name="newPriority">The new priority.</param>
        public void ChangePriority(Domain.Enums.TaskPriority newPriority)
        {
            Priority = newPriority;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Assigns the task to a user.
        /// </summary>
        /// <param name="userId">The ID of the user to assign to, or null to unassign.</param>
        public void AssignTo(Guid? userId)
        {
            AssigneeId = userId;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the task title and description.
        /// </summary>
        /// <param name="newTitle">The new title.</param>
        /// <param name="newDescription">The new description.</param>
        public void UpdateDetails(string newTitle, string? newDescription)
        {
            Title = newTitle;
            Description = newDescription;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets the due date for the task.
        /// </summary>
        /// <param name="dueDate">The due date, or null to remove.</param>
        public void SetDueDate(DateTime? dueDate)
        {
            DueDate = dueDate;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
