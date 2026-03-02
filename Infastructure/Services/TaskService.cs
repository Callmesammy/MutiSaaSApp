using Application.Constants;
using Application.DTOs.Common;
using Application.DTOs.Task;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Infastructure.Services
{
    /// <summary>
    /// Service for handling task operations.
    /// Manages task creation, retrieval, updates, and deletion with organization scoping.
    /// Includes distributed caching for performance optimization.
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<TaskService> _logger;
        private const int TaskCacheDurationMinutes = 10;

        /// <summary>
        /// Initializes a new instance of the TaskService class.
        /// </summary>
        public TaskService(
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            ICacheService cacheService,
            ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new task in the organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="userId">The ID of the user creating the task.</param>
        /// <param name="request">The create task request.</param>
        /// <returns>The created task response.</returns>
        public async Task<TaskResponse> CreateTaskAsync(Guid organizationId, Guid userId, CreateTaskRequest request)
        {
            // Verify organization exists
            var organization = await _organizationRepository.GetByIdAsync(organizationId);
            if (organization == null)
            {
                throw new NotFoundException("Organization", organizationId);
            }

            // Verify user exists and belongs to organization
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User", userId);
            }

            // Create the task
            var taskItem = new TaskItem(
                title: request.Title,
                organizationId: organizationId,
                createdByUserId: userId,
                status: Domain.Enums.TaskStatus.Todo,
                priority: request.Priority)
            {
                Description = request.Description,
                AssigneeId = request.AssigneeId,
                DueDate = request.DueDate
            };

            // If task is being assigned, verify the assignee is in the organization
            if (request.AssigneeId.HasValue)
            {
                var assignee = await _userRepository.GetByIdAsync(request.AssigneeId.Value);
                if (assignee == null)
                {
                    throw new NotFoundException("User", request.AssigneeId.Value);
                }
            }

            await _taskRepository.AddAsync(taskItem);
            await _taskRepository.SaveChangesAsync();

            // Invalidate cache for organization tasks
            await _cacheService.RemoveAsync(CacheKeys.GetOrgTasksKey(organizationId));
            _logger.LogInformation("Task created and cache invalidated: {TaskId}", taskItem.Id);

            return MapToTaskResponse(taskItem, user, null);
        }

        /// <summary>
        /// Gets a task by ID, verifying it belongs to the specified organization.
        /// Results are cached to improve performance.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="taskId">The task ID to retrieve.</param>
        /// <returns>The task response, or null if not found.</returns>
        public async Task<TaskResponse?> GetTaskAsync(Guid organizationId, Guid taskId)
        {
            var cacheKey = CacheKeys.GetTaskKey(organizationId, taskId);

            // Try to get from cache first
            var cachedTask = await _cacheService.GetAsync<TaskResponse>(cacheKey);
            if (cachedTask != null)
            {
                _logger.LogDebug("Task retrieved from cache: {TaskId}", taskId);
                return cachedTask;
            }

            var task = await _taskRepository.GetByIdAsync(taskId);

            if (task == null || task.OrganizationId != organizationId)
            {
                return null;
            }

            var createdByUser = await _userRepository.GetByIdAsync(task.CreatedByUserId);
            var assignee = task.AssigneeId.HasValue ? await _userRepository.GetByIdAsync(task.AssigneeId.Value) : null;

            var response = MapToTaskResponse(task, createdByUser, assignee);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, response, TaskCacheDurationMinutes);

            return response;
        }

        /// <summary>
        /// Gets all tasks for an organization.
        /// Results are cached to improve performance.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <returns>A list of all tasks in the organization.</returns>
        public async Task<List<TaskResponse>> GetTasksAsync(Guid organizationId)
        {
            var cacheKey = CacheKeys.GetOrgTasksKey(organizationId);

            // Try to get from cache first
            var cachedTasks = await _cacheService.GetAsync<List<TaskResponse>>(cacheKey);
            if (cachedTasks != null)
            {
                _logger.LogDebug("Tasks retrieved from cache for organization: {OrgId}", organizationId);
                return cachedTasks;
            }

            var tasks = await _taskRepository.GetTasksByOrganizationAsync(organizationId);
            var response = tasks.Select(t => MapToTaskResponse(t, t.CreatedByUser, t.Assignee)).ToList();

            // Cache the result
            await _cacheService.SetAsync(cacheKey, response, TaskCacheDurationMinutes);

            return response;
        }

        /// <summary>
        /// Gets tasks for an organization with optional filtering.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="status">Optional status filter.</param>
        /// <param name="priority">Optional priority filter.</param>
        /// <param name="assigneeId">Optional assignee filter.</param>
        /// <returns>A list of filtered tasks.</returns>
        public async Task<List<TaskResponse>> GetTasksFilteredAsync(
            Guid organizationId,
            Domain.Enums.TaskStatus? status = null,
            Domain.Enums.TaskPriority? priority = null,
            Guid? assigneeId = null)
        {
            var tasks = await _taskRepository.GetTasksFilteredAsync(organizationId, status, priority);

            if (assigneeId.HasValue)
            {
                tasks = tasks.Where(t => t.AssigneeId == assigneeId.Value).ToList();
            }

            return tasks.Select(t => MapToTaskResponse(t, t.CreatedByUser, t.Assignee)).ToList();
        }

        /// <summary>
        /// Updates an existing task.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="taskId">The task ID to update.</param>
        /// <param name="userId">The ID of the user performing the update.</param>
        /// <param name="userRole">The role of the user performing the update.</param>
        /// <param name="request">The update task request.</param>
        /// <returns>The updated task response.</returns>
        public async Task<TaskResponse> UpdateTaskAsync(
            Guid organizationId,
            Guid taskId,
            Guid userId,
            UserRole userRole,
            UpdateTaskRequest request)
        {
            // Get the task
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null || task.OrganizationId != organizationId)
            {
                throw new NotFoundException("Task", taskId);
            }

            // Only admins can update task status
            if (request.Status.HasValue && userRole != UserRole.Admin)
            {
                throw new UnauthorizedException("Only admins can change task status.");
            }

            // Update task fields
            if (!string.IsNullOrEmpty(request.Title))
            {
                task.UpdateDetails(request.Title, request.Description ?? task.Description);
            }
            else if (!string.IsNullOrEmpty(request.Description))
            {
                task.Description = request.Description;
            }

            if (request.Status.HasValue)
            {
                task.ChangeStatus(request.Status.Value);
            }

            if (request.Priority.HasValue)
            {
                task.ChangePriority(request.Priority.Value);
            }

            if (request.AssigneeId.HasValue)
            {
                // If AssigneeId is Guid.Empty, unassign
                if (request.AssigneeId == Guid.Empty)
                {
                    task.AssignTo(null);
                }
                else
                {
                    // Verify assignee exists
                    var assigneeUser = await _userRepository.GetByIdAsync(request.AssigneeId.Value);
                    if (assigneeUser == null)
                    {
                        throw new NotFoundException("User", request.AssigneeId.Value);
                    }
                    task.AssignTo(request.AssigneeId.Value);
                }
            }

            if (request.DueDate.HasValue)
            {
                task.SetDueDate(request.DueDate);
            }

            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveChangesAsync();

            // Invalidate cache for this task and org tasks list
            await _cacheService.RemoveAsync(CacheKeys.GetTaskKey(organizationId, taskId));
            await _cacheService.RemoveAsync(CacheKeys.GetOrgTasksKey(organizationId));
            _logger.LogInformation("Task updated and cache invalidated: {TaskId}", taskId);

            var createdByUser = await _userRepository.GetByIdAsync(task.CreatedByUserId);
            var assigneeUpdated = task.AssigneeId.HasValue ? await _userRepository.GetByIdAsync(task.AssigneeId.Value) : null;

            return MapToTaskResponse(task, createdByUser, assigneeUpdated);
        }

        /// <summary>
        /// Deletes (soft-deletes) a task. Only admins can delete tasks.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="taskId">The task ID to delete.</param>
        /// <returns>True if the task was successfully deleted.</returns>
        public async Task<bool> DeleteTaskAsync(Guid organizationId, Guid taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null || task.OrganizationId != organizationId)
            {
                throw new NotFoundException("Task", taskId);
            }

            await _taskRepository.DeleteAsync(task);
            await _taskRepository.SaveChangesAsync();

            // Invalidate cache for this task and org tasks list
            await _cacheService.RemoveAsync(CacheKeys.GetTaskKey(organizationId, taskId));
            await _cacheService.RemoveAsync(CacheKeys.GetOrgTasksKey(organizationId));
            _logger.LogInformation("Task deleted and cache invalidated: {TaskId}", taskId);

            return true;
        }

        /// <summary>
        /// Gets the total count of tasks in an organization.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        public async Task<int> GetTaskCountAsync(Guid organizationId)
        {
            return await _taskRepository.GetTaskCountByOrganizationAsync(organizationId);
        }

        /// <summary>
        /// Retrieves paginated and filtered tasks for an organization.
        /// Supports advanced filtering by status, priority, assignee, date ranges, and search.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="request">The pagination and filter request.</param>
        /// <returns>A paginated response containing filtered tasks.</returns>
        public async Task<PaginatedResponse<TaskResponse>> GetTasksPaginatedAsync(Guid organizationId, GetTasksRequest request)
        {
            // Normalize pagination parameters
            request.Normalize();

            // Get all tasks for the organization with related data
            var tasks = await _taskRepository.GetTasksByOrganizationAsync(organizationId);

            // Apply filters
            var query = tasks.AsEnumerable();

            if (request.Status.HasValue)
            {
                query = query.Where(t => t.Status == request.Status.Value);
            }

            if (request.Priority.HasValue)
            {
                query = query.Where(t => t.Priority == request.Priority.Value);
            }

            if (request.AssigneeId.HasValue)
            {
                query = query.Where(t => t.AssigneeId == request.AssigneeId.Value);
            }

            if (request.CreatedByUserId.HasValue)
            {
                query = query.Where(t => t.CreatedByUserId == request.CreatedByUserId.Value);
            }

            if (request.CreatedAfter.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= request.CreatedAfter.Value);
            }

            if (request.CreatedBefore.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= request.CreatedBefore.Value);
            }

            if (request.DueAfter.HasValue)
            {
                query = query.Where(t => t.DueDate.HasValue && t.DueDate >= request.DueAfter.Value);
            }

            if (request.DueBefore.HasValue)
            {
                query = query.Where(t => t.DueDate.HasValue && t.DueDate <= request.DueBefore.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(t =>
                    t.Title.ToLower().Contains(searchTerm) ||
                    (t.Description?.ToLower().Contains(searchTerm) ?? false)
                );
            }

            // Get total count before pagination
            var totalCount = query.Count();

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortDirection);

            // Apply pagination
            var paginatedTasks = query.Skip(request.Skip).Take(request.Take).ToList();

            // Map to response DTOs
            var responses = paginatedTasks.Select(task =>
                MapToTaskResponse(task, task.CreatedByUser, task.Assignee)
            ).ToList();

            _logger.LogInformation("Retrieved {Count} paginated tasks for organization {OrgId}", responses.Count, organizationId);

            return new PaginatedResponse<TaskResponse>(responses, totalCount, request.Skip, request.Take);
        }

        /// <summary>
        /// Applies sorting to the task query based on sort parameters.
        /// </summary>
        private static IEnumerable<TaskItem> ApplySorting(IEnumerable<TaskItem> query, string? sortBy, string? sortDirection)
        {
            var isDescending = sortDirection?.ToLower() == "desc";

            return (sortBy?.ToLower()) switch
            {
                "status" => isDescending 
                    ? query.OrderByDescending(t => t.Status) 
                    : query.OrderBy(t => t.Status),

                "priority" => isDescending 
                    ? query.OrderByDescending(t => t.Priority) 
                    : query.OrderBy(t => t.Priority),

                "duedate" => isDescending 
                    ? query.OrderByDescending(t => t.DueDate) 
                    : query.OrderBy(t => t.DueDate),

                _ => isDescending 
                    ? query.OrderByDescending(t => t.CreatedAt) 
                    : query.OrderBy(t => t.CreatedAt)
            };
        }

        /// 
        /// Maps a TaskItem domain entity to a TaskResponse DTO.
        /// </summary>
        private TaskResponse MapToTaskResponse(TaskItem task, User? createdByUser, User? assignee)
        {
            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                OrganizationId = task.OrganizationId,
                CreatedByUserId = task.CreatedByUserId,
                CreatedByEmail = createdByUser?.Email ?? "Unknown",
                AssigneeId = task.AssigneeId,
                AssigneeEmail = assignee?.Email,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}
