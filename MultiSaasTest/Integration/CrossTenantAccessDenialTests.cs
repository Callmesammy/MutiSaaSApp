using Application.DTOs.Task;
using Domain.Entities;
using Domain.Enums;
using Infastructure.Repositories;
using Infastructure.Services;
using MultiSaasTest.Fixtures;

namespace MultiSaasTest.Integration
{
    /// <summary>
    /// Integration tests for Cross-Tenant Access Denial - Feature #5
    /// Validates that users cannot access or modify data from other organizations.
    /// </summary>
    public class CrossTenantAccessDenialTests : IDisposable
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly TaskService _taskService;
        private Organization _org1;
        private Organization _org2;
        private User _org1Admin;
        private User _org2Admin;

        public CrossTenantAccessDenialTests()
        {
            _fixture = new TestDatabaseFixture();

            var taskRepo = new TaskRepository(_fixture.Context);
            var userRepo = new UserRepository(_fixture.Context);
            var orgRepo = new OrganizationRepository(_fixture.Context);

            _taskService = new TaskService(taskRepo, userRepo, orgRepo);
            SetupTwoOrganizations();
        }

        private void SetupTwoOrganizations()
        {
            _org1 = TestDataFactory.CreateOrganization("Org 1");
            _org2 = TestDataFactory.CreateOrganization("Org 2");
            _org1Admin = TestDataFactory.CreateUser("admin@org1.com", "hash", "Admin", "Org1");
            _org2Admin = TestDataFactory.CreateUser("admin@org2.com", "hash", "Admin", "Org2");

            _fixture.Context.Organizations.Add(_org1);
            _fixture.Context.Organizations.Add(_org2);
            _fixture.Context.Users.Add(_org1Admin);
            _fixture.Context.Users.Add(_org2Admin);

            _fixture.Context.OrgUsers.Add(TestDataFactory.CreateOrgUser(_org1.Id, _org1Admin.Id, UserRole.Admin));
            _fixture.Context.OrgUsers.Add(TestDataFactory.CreateOrgUser(_org2.Id, _org2Admin.Id, UserRole.Admin));

            _fixture.Context.SaveChanges();
        }

        [Fact]
        public async Task Org1Admin_CannotAccessOrg2Tasks()
        {
            // Arrange: Org2 admin creates a task in Org2
            var createRequest = new CreateTaskRequest { Title = "Org2 Secret Task", Priority = TaskPriority.High };
            var org2Task = await _taskService.CreateTaskAsync(_org2.Id, _org2Admin.Id, createRequest);

            // Act: Org1 admin tries to access Org2's task using Org2's task ID with their org context
            var retrievedTask = await _taskService.GetTaskAsync(_org1.Id, org2Task.Id);

            // Assert: Should return null (task doesn't exist in Org1's context)
            Assert.Null(retrievedTask);
        }

        [Fact]
        public async Task Org1Admin_CannotUpdateOrg2Tasks()
        {
            // Arrange: Org2 admin creates a task
            var createRequest = new CreateTaskRequest { Title = "Org2 Task", Priority = TaskPriority.Medium };
            var org2Task = await _taskService.CreateTaskAsync(_org2.Id, _org2Admin.Id, createRequest);

            // Act: Org1 admin tries to update Org2's task (will be rejected at service level)
            var updateRequest = new UpdateTaskRequest { Title = "Hacked Title" };
            
            // Assert: Should throw NotFoundException because task doesn't exist in Org1 context
            await Assert.ThrowsAsync<Domain.Exceptions.NotFoundException>(async () =>
                await _taskService.UpdateTaskAsync(_org1.Id, org2Task.Id, _org1Admin.Id, UserRole.Admin, updateRequest));
        }

        [Fact]
        public async Task Org1Admin_CannotDeleteOrg2Tasks()
        {
            // Arrange: Org2 admin creates a task
            var createRequest = new CreateTaskRequest { Title = "Org2 Task to Delete", Priority = TaskPriority.Low };
            var org2Task = await _taskService.CreateTaskAsync(_org2.Id, _org2Admin.Id, createRequest);

            // Act: Org1 admin tries to delete Org2's task
            // Assert: Should throw NotFoundException
            await Assert.ThrowsAsync<Domain.Exceptions.NotFoundException>(async () =>
                await _taskService.DeleteTaskAsync(_org1.Id, org2Task.Id));
        }

        [Fact]
        public async Task Org1_TaskListDoesNotLeakOrg2Tasks()
        {
            // Arrange: Both orgs have tasks
            var org1Task = TestDataFactory.CreateTask("Org1 Task", _org1.Id, _org1Admin.Id);
            var org2Task = TestDataFactory.CreateTask("Org2 Task", _org2.Id, _org2Admin.Id);

            _fixture.Context.TaskItems.Add(org1Task);
            _fixture.Context.TaskItems.Add(org2Task);
            _fixture.Context.SaveChanges();

            // Act: Get all tasks for Org1
            var org1Tasks = await _taskService.GetTasksAsync(_org1.Id);

            // Assert: Should only see Org1's task
            Assert.Single(org1Tasks);
            Assert.Equal("Org1 Task", org1Tasks.First().Title);
            Assert.DoesNotContain("Org2 Task", org1Tasks.Select(t => t.Title));
        }

        [Fact]
        public async Task FilteredTaskQuery_OnlyReturnsOrgSpecificResults()
        {
            // Arrange: Create high-priority tasks in both orgs
            var org1HighTask = new CreateTaskRequest { Title = "Org1 High Priority", Priority = TaskPriority.High };
            var org2HighTask = new CreateTaskRequest { Title = "Org2 High Priority", Priority = TaskPriority.High };

            await _taskService.CreateTaskAsync(_org1.Id, _org1Admin.Id, org1HighTask);
            await _taskService.CreateTaskAsync(_org2.Id, _org2Admin.Id, org2HighTask);

            // Act: Query Org1 for high priority tasks
            var org1HighPriority = await _taskService.GetTasksFilteredAsync(_org1.Id, null, TaskPriority.High);

            // Assert: Should only see Org1's high priority task
            Assert.Single(org1HighPriority);
            Assert.Equal("Org1 High Priority", org1HighPriority.First().Title);
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
