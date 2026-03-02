using Application.DTOs.Common;
using Domain.Enums;

namespace Application.DTOs.Task
{
    /// <summary>
    /// Advanced task filtering and pagination request.
    /// Combines pagination parameters with task-specific filters.
    /// </summary>
    public class GetTasksRequest : PaginationRequest
    {
        /// <summary>
        /// Gets or sets the task status filter (optional).
        /// </summary>
        public Domain.Enums.TaskStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the task priority filter (optional).
        /// </summary>
        public Domain.Enums.TaskPriority? Priority { get; set; }

        /// <summary>
        /// Gets or sets the assignee ID filter (optional).
        /// </summary>
        public Guid? AssigneeId { get; set; }

        /// <summary>
        /// Gets or sets the created by user ID filter (optional).
        /// </summary>
        public Guid? CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the start date filter for task creation (optional).
        /// </summary>
        public DateTime? CreatedAfter { get; set; }

        /// <summary>
        /// Gets or sets the end date filter for task creation (optional).
        /// </summary>
        public DateTime? CreatedBefore { get; set; }

        /// <summary>
        /// Gets or sets the start date filter for task due date (optional).
        /// </summary>
        public DateTime? DueAfter { get; set; }

        /// <summary>
        /// Gets or sets the end date filter for task due date (optional).
        /// </summary>
        public DateTime? DueBefore { get; set; }

        /// <summary>
        /// Gets or sets a search term to filter by task title or description (optional).
        /// </summary>
        public string? SearchTerm { get; set; }
    }
}
