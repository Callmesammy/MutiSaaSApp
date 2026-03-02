using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infastructure.Repositories;
using Infastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using MultiSaasTest.Fixtures;

namespace MultiSaasTest.Services
{
    /// <summary>
    /// Unit tests for TaskService - Feature #4: Task Management
    /// Tests task CRUD operations with role-based permissions and org isolation.
    /// </summary>
    public class TaskServiceTests : IDisposable
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly TaskService _taskService;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<ILogger<TaskService>> _loggerMock;
        private Organization _org;
        private User _adminUser;
        private User _memberUser;
        private Guid _adminId;
        private Guid _memberId;

        public TaskServiceTests()
        {
            _fixture = new TestDatabaseFixture();
            _cacheServiceMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger<TaskService>>();

            var taskRepo = new TaskRepository(_fixture.Context);
            var userRepo = new UserRepository(_fixture.Context);
            var orgRepo = new OrganizationRepository(_fixture.Context);

            _taskService = new TaskService(
                taskRepository: taskRepo,
                userRepository: userRepo,
                organizationRepository: orgRepo,
                cacheService: _cacheServiceMock.Object,
                logger: _loggerMock.Object);

            SetupTestData();
        }

        private void SetupTestData()
        {
            _org = TestDataFactory.CreateOrganization("Test Org");
            _adminUser = TestDataFactory.CreateUser("admin@test.com", "hash", "Admin", "User");
            _memberUser = TestDataFactory.CreateUser("member@test.com", "hash", "Member", "User");

            _fixture.Context.Organizations.Add(_org);
            _fixture.Context.Users.Add(_adminUser);
            _fixture.Context.Users.Add(_memberUser);

            var adminOrgUser = TestDataFactory.CreateOrgUser(_org.Id, _adminUser.Id, UserRole.Admin);
            var memberOrgUser = TestDataFactory.CreateOrgUser(_org.Id, _memberUser.Id, UserRole.Member);
            _fixture.Context.OrgUsers.Add(adminOrgUser);
            _fixture.Context.OrgUsers.Add(memberOrgUser);
            _fixture.Context.SaveChanges();

            _adminId = _adminUser.Id;
            _memberId = _memberUser.Id;
        }

        [Fact]
        public async Task CreateTaskAsync_WithValidData_CreatesTaskAndReturnsResponse()
        {
            // Arrange
            var request = new Application.DTOs.Task.CreateTaskRequest
            {
                Title = "New Task",
                Description = "Task description",
                Priority = TaskPriority.High
            };

            // Act
            var result = await _taskService.CreateTaskAsync(_org.Id, _adminId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Task", result.Title);
            Assert.Equal(Domain.Enums.TaskStatus.Todo, result.Status);
            Assert.Equal(TaskPriority.High, result.Priority);
        }

        [Fact]
        public async Task CreateTaskAsync_WithAssignee_AssignsTaskToUser()
        {
            // Arrange
            var request = new Application.DTOs.Task.CreateTaskRequest
            {
                Title = "Assigned Task",
                Priority = TaskPriority.Medium,
                AssigneeId = _memberId
            };

            // Act
            var result = await _taskService.CreateTaskAsync(_org.Id, _adminId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_memberId, result.AssigneeId);
        }

        [Fact]
        public async Task CreateTaskAsync_WithNonExistentOrganization_ThrowsNotFoundException()
        {
            // Arrange
            var request = new Application.DTOs.Task.CreateTaskRequest { Title = "Task" };
            var fakeOrgId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await _taskService.CreateTaskAsync(fakeOrgId, _adminId, request));
        }

        [Fact]
        public async Task CreateTaskAsync_WithNonExistentUser_ThrowsNotFoundException()
        {
            // Arrange
            var request = new Application.DTOs.Task.CreateTaskRequest { Title = "Task" };
            var fakeUserId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await _taskService.CreateTaskAsync(_org.Id, fakeUserId, request));
        }

        [Fact]
        public async Task GetTaskAsync_WithValidId_ReturnsTask()
        {
            // Arrange
            var task = TestDataFactory.CreateTask("Test Task", _org.Id, _adminId);
            _fixture.Context.TaskItems.Add(task);
            _fixture.Context.SaveChanges();

            // Act
            var result = await _taskService.GetTaskAsync(_org.Id, task.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Task", result.Title);
        }

        [Fact]
        public async Task GetTaskAsync_WithDifferentOrganization_ReturnsNull()
        {
            // Arrange
            var task = TestDataFactory.CreateTask("Test Task", _org.Id, _adminId);
            _fixture.Context.TaskItems.Add(task);
            _fixture.Context.SaveChanges();

            var differentOrgId = Guid.NewGuid();

            // Act
            var result = await _taskService.GetTaskAsync(differentOrgId, task.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithAdminRole_CanChangeStatus()
        {
            // Arrange
            var task = TestDataFactory.CreateTask("Test Task", _org.Id, _adminId, Domain.Enums.TaskStatus.Todo);
            _fixture.Context.TaskItems.Add(task);
            _fixture.Context.SaveChanges();

            var request = new Application.DTOs.Task.UpdateTaskRequest
            {
                Status = Domain.Enums.TaskStatus.InProgress
            };

            // Act
            var result = await _taskService.UpdateTaskAsync(_org.Id, task.Id, _adminId, UserRole.Admin, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Domain.Enums.TaskStatus.InProgress, result.Status);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithMemberRole_CannotChangeStatus()
        {
            // Arrange
            var task = TestDataFactory.CreateTask("Test Task", _org.Id, _adminId, Domain.Enums.TaskStatus.Todo);
            _fixture.Context.TaskItems.Add(task);
            _fixture.Context.SaveChanges();

            var request = new Application.DTOs.Task.UpdateTaskRequest
            {
                Status = Domain.Enums.TaskStatus.InProgress
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _taskService.UpdateTaskAsync(_org.Id, task.Id, _memberId, UserRole.Member, request));
        }

        [Fact]
        public async Task DeleteTaskAsync_WithValidId_SoftDeletesTask()
        {
            // Arrange
            var task = TestDataFactory.CreateTask("Test Task", _org.Id, _adminId);
            _fixture.Context.TaskItems.Add(task);
            _fixture.Context.SaveChanges();

            // Act
            var result = await _taskService.DeleteTaskAsync(_org.Id, task.Id);

            // Assert
            Assert.True(result);
            var deletedTask = await _fixture.Context.TaskItems.FindAsync(task.Id);
            Assert.True(deletedTask.IsDeleted);
        }

        [Fact]
        public async Task GetTasksAsync_ReturnsAllOrgTasks()
        {
            // Arrange
            var task1 = TestDataFactory.CreateTask("Task 1", _org.Id, _adminId);
            var task2 = TestDataFactory.CreateTask("Task 2", _org.Id, _adminId);
            _fixture.Context.TaskItems.Add(task1);
            _fixture.Context.TaskItems.Add(task2);
            _fixture.Context.SaveChanges();

            // Act
            var result = await _taskService.GetTasksAsync(_org.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
