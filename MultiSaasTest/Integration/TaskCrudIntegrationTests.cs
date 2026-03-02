using Application.DTOs.Task;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infastructure.Repositories;
using Infastructure.Services;
using MultiSaasTest.Fixtures;

namespace MultiSaasTest.Integration
{
    /// <summary>
    /// Integration tests for Task endpoints - Feature #4
    /// Tests complete task CRUD workflow through the service layer.
    /// </summary>
    public class TaskCrudIntegrationTests : IDisposable
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly TaskService _taskService;
        private Organization _org;
        private User _admin;
        private User _member;

        public TaskCrudIntegrationTests()
        {
            _fixture = new TestDatabaseFixture();

            var taskRepo = new TaskRepository(_fixture.Context);
            var userRepo = new UserRepository(_fixture.Context);
            var orgRepo = new OrganizationRepository(_fixture.Context);

            _taskService = new TaskService(taskRepo, userRepo, orgRepo);
            SetupTestData();
        }

        private void SetupTestData()
        {
            _org = TestDataFactory.CreateOrganization("Integration Test Org");
            _admin = TestDataFactory.CreateUser("admin@test.com", "hash", "Admin", "User");
            _member = TestDataFactory.CreateUser("member@test.com", "hash", "Member", "User");

            _fixture.Context.Organizations.Add(_org);
            _fixture.Context.Users.Add(_admin);
            _fixture.Context.Users.Add(_member);

            _fixture.Context.OrgUsers.Add(TestDataFactory.CreateOrgUser(_org.Id, _admin.Id, UserRole.Admin));
            _fixture.Context.OrgUsers.Add(TestDataFactory.CreateOrgUser(_org.Id, _member.Id, UserRole.Member));

            _fixture.Context.SaveChanges();
        }

        [Fact]
        public async Task CreateTaskFlow_AdminCreatesTaskAndAssignsToMember()
        {
            // Step 1: Admin creates task
            var createRequest = new CreateTaskRequest
            {
                Title = "Implement Feature X",
                Description = "Build the new feature",
                Priority = TaskPriority.High,
                AssigneeId = _member.Id,
                DueDate = DateTime.UtcNow.AddDays(5)
            };

            var createdTask = await _taskService.CreateTaskAsync(_org.Id, _admin.Id, createRequest);

            Assert.NotNull(createdTask);
            Assert.Equal("Implement Feature X", createdTask.Title);
            Assert.Equal(Domain.Enums.TaskStatus.Todo, createdTask.Status);
            Assert.Equal(_member.Id, createdTask.AssigneeId);

            // Step 2: Retrieve created task
            var retrievedTask = await _taskService.GetTaskAsync(_org.Id, createdTask.Id);

            Assert.NotNull(retrievedTask);
            Assert.Equal(createdTask.Id, retrievedTask.Id);
        }

        [Fact]
        public async Task UpdateTaskFlow_AdminChangesStatusMemberCannot()
        {
            // Arrange: Create task
            var createRequest = new CreateTaskRequest { Title = "Task", Priority = TaskPriority.Medium };
            var task = await _taskService.CreateTaskAsync(_org.Id, _admin.Id, createRequest);

            // Act Step 1: Admin changes status to InProgress
            var updateRequest = new UpdateTaskRequest { Status = Domain.Enums.TaskStatus.InProgress };
            var updatedTask = await _taskService.UpdateTaskAsync(_org.Id, task.Id, _admin.Id, UserRole.Admin, updateRequest);

            Assert.Equal(Domain.Enums.TaskStatus.InProgress, updatedTask.Status);

            // Act Step 2: Try member changing status to Done (should fail)
            var memberUpdateRequest = new UpdateTaskRequest { Status = Domain.Enums.TaskStatus.Done };

            // Assert
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _taskService.UpdateTaskAsync(_org.Id, task.Id, _member.Id, UserRole.Member, memberUpdateRequest));
        }

        [Fact]
        public async Task FilterTasksFlow_RetrievesTasksByStatusAndPriority()
        {
            // Arrange: Create multiple tasks with different statuses/priorities
            var todoHighTask = new CreateTaskRequest { Title = "Urgent Bug", Priority = TaskPriority.High };
            var todoLowTask = new CreateTaskRequest { Title = "Nice to Have", Priority = TaskPriority.Low };

            var task1 = await _taskService.CreateTaskAsync(_org.Id, _admin.Id, todoHighTask);
            var task2 = await _taskService.CreateTaskAsync(_org.Id, _admin.Id, todoLowTask);
            var task3 = await _taskService.CreateTaskAsync(_org.Id, _admin.Id, new CreateTaskRequest { Title = "In Progress", Priority = TaskPriority.High });

            // Move task3 to InProgress
            var updateToInProgress = new UpdateTaskRequest { Status = Domain.Enums.TaskStatus.InProgress };
            await _taskService.UpdateTaskAsync(_org.Id, task3.Id, _admin.Id, UserRole.Admin, updateToInProgress);

            // Act & Assert: Get all high priority tasks
            var highPriorityTasks = await _taskService.GetTasksFilteredAsync(_org.Id, null, TaskPriority.High);
            Assert.Equal(2, highPriorityTasks.Count);

            // Act & Assert: Get all Todo tasks
            var todoTasks = await _taskService.GetTasksFilteredAsync(_org.Id, Domain.Enums.TaskStatus.Todo);
            Assert.Equal(2, todoTasks.Count);
        }

        [Fact]
        public async Task DeleteTaskFlow_AdminDeletesTaskSoftDelete()
        {
            // Arrange
            var createRequest = new CreateTaskRequest { Title = "Task to Delete", Priority = TaskPriority.Medium };
            var task = await _taskService.CreateTaskAsync(_org.Id, _admin.Id, createRequest);

            // Act: Delete task
            var deleted = await _taskService.DeleteTaskAsync(_org.Id, task.Id);

            Assert.True(deleted);

            // Assert: Task should not appear in queries (soft delete)
            var retrievedTask = await _taskService.GetTaskAsync(_org.Id, task.Id);
            Assert.Null(retrievedTask);

            var allTasks = await _taskService.GetTasksAsync(_org.Id);
            Assert.DoesNotContain(task.Id, allTasks.Select(t => t.Id));
        }

        [Fact]
        public async Task AssignmentFlow_TaskCanBeAssignedAndUnassigned()
        {
            // Arrange: Create unassigned task
            var createRequest = new CreateTaskRequest { Title = "Unassigned Task", Priority = TaskPriority.Medium };
            var task = await _taskService.CreateTaskAsync(_org.Id, _admin.Id, createRequest);

            // Act Step 1: Assign to member
            var assignRequest = new UpdateTaskRequest { AssigneeId = _member.Id };
            var assignedTask = await _taskService.UpdateTaskAsync(_org.Id, task.Id, _admin.Id, UserRole.Admin, assignRequest);

            Assert.Equal(_member.Id, assignedTask.AssigneeId);

            // Act Step 2: Unassign (set to Guid.Empty)
            var unassignRequest = new UpdateTaskRequest { AssigneeId = Guid.Empty };
            var unassignedTask = await _taskService.UpdateTaskAsync(_org.Id, task.Id, _admin.Id, UserRole.Admin, unassignRequest);

            Assert.Null(unassignedTask.AssigneeId);
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
