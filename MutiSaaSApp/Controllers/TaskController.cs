using Application.Constants;
using Application.DTOs.Task;
using Application.Interfaces;
using Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MutiSaaSApp.Common;

namespace MutiSaaSApp.Controllers
{
    /// <summary>
    /// Controller for task management endpoints.
    /// Handles CRUD operations for tasks scoped to organizations.
    /// All endpoints require authentication and organization membership.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : BaseAuthController
    {
        private readonly ITaskService _taskService;
        private readonly IValidator<CreateTaskRequest> _createValidator;
        private readonly IValidator<UpdateTaskRequest> _updateValidator;
        private readonly ILogger<TaskController> _logger;

        /// <summary>
        /// Initializes a new instance of the TaskController class.
        /// </summary>
        public TaskController(
            ITaskService taskService,
            IValidator<CreateTaskRequest> createValidator,
            IValidator<UpdateTaskRequest> updateValidator,
            ILogger<TaskController> logger)
        {
            _taskService = taskService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new task in the organization.
        /// </summary>
        /// <param name="request">The create task request.</param>
        /// <returns>201 Created with the created task data.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TaskResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            _logger.LogInformation("Create task request: {Title}", request.Title);

            // Get organization and user from claims
            var organizationId = GetOrganizationId();
            var userId = GetUserId();

            // Validate request
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(e => e.ErrorMessage)));

                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed.", errors));
            }

            try
            {
                var result = await _taskService.CreateTaskAsync(organizationId, userId, request);
                _logger.LogInformation("Task created successfully: {TaskId}", result.Id);
                return CreatedAtAction(nameof(GetTask), new { id = result.Id }, 
                    ApiResponse<TaskResponse>.SuccessResponse(result, "Task created successfully."));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating task");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific task by ID.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <returns>200 OK with task data.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<TaskResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTask([FromRoute] Guid id)
        {
            _logger.LogInformation("Get task request: {TaskId}", id);

            var organizationId = GetOrganizationId();

            var task = await _taskService.GetTaskAsync(organizationId, id);
            if (task == null)
            {
                _logger.LogWarning("Task not found: {TaskId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse($"Task with id '{id}' was not found."));
            }

            return Ok(ApiResponse<TaskResponse>.SuccessResponse(task));
        }

        /// <summary>
        /// Retrieves all tasks in the organization.
        /// Optional query parameters for filtering by status and priority.
        /// </summary>
        /// <param name="status">Filter by status (optional).</param>
        /// <param name="priority">Filter by priority (optional).</param>
        /// <param name="assigneeId">Filter by assignee ID (optional).</param>
        /// <returns>200 OK with list of tasks.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<TaskResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTasks(
            [FromQuery] Domain.Enums.TaskStatus? status = null,
            [FromQuery] Domain.Enums.TaskPriority? priority = null,
            [FromQuery] Guid? assigneeId = null)
        {
            _logger.LogInformation("Get tasks request with filters - Status: {Status}, Priority: {Priority}, Assignee: {Assignee}",
                status, priority, assigneeId);

            var organizationId = GetOrganizationId();

            var tasks = await _taskService.GetTasksFilteredAsync(organizationId, status, priority, assigneeId);
            _logger.LogInformation("Retrieved {Count} tasks", tasks.Count);

            return Ok(ApiResponse<List<TaskResponse>>.SuccessResponse(tasks));
        }

        /// <summary>
        /// Updates an existing task.
        /// Only admins can change task status.
        /// </summary>
        /// <param name="id">The task ID to update.</param>
        /// <param name="request">The update task request.</param>
        /// <returns>200 OK with updated task data.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<TaskResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTask(
            [FromRoute] Guid id,
            [FromBody] UpdateTaskRequest request)
        {
            _logger.LogInformation("Update task request: {TaskId}", id);

            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var userRole = GetUserRole();

            // Validate request
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(e => e.ErrorMessage)));

                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed.", errors));
            }

            try
            {
                var result = await _taskService.UpdateTaskAsync(organizationId, id, userId, userRole, request);
                _logger.LogInformation("Task updated successfully: {TaskId}", id);
                return Ok(ApiResponse<TaskResponse>.SuccessResponse(result, "Task updated successfully."));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning("Unauthorized: {Message}", ex.Message);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating task");
                throw;
            }
        }

        /// <summary>
        /// Deletes a task.
        /// Only admins can delete tasks.
        /// </summary>
        /// <param name="id">The task ID to delete.</param>
        /// <returns>204 No Content on success.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTask([FromRoute] Guid id)
        {
            _logger.LogInformation("Delete task request: {TaskId}", id);

            var organizationId = GetOrganizationId();

            try
            {
                await _taskService.DeleteTaskAsync(organizationId, id);
                _logger.LogInformation("Task deleted successfully: {TaskId}", id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting task");
                throw;
            }
        }
    }
}
