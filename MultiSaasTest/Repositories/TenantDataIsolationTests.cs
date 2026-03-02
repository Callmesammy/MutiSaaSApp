using Domain.Entities;
using Domain.Enums;
using Infastructure.Repositories;
using MultiSaasTest.Fixtures;

namespace MultiSaasTest.Repositories
{
    /// <summary>
    /// Unit tests for Tenant Data Isolation - Feature #5
    /// Validates that repositories enforce organization boundaries.
    /// </summary>
    public class TenantDataIsolationTests : IDisposable
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly TaskRepository _taskRepository;
        private readonly UserRepository _userRepository;
        private readonly OrgUserRepository _orgUserRepository;

        public TenantDataIsolationTests()
        {
            _fixture = new TestDatabaseFixture();
            _taskRepository = new TaskRepository(_fixture.Context);
            _userRepository = new UserRepository(_fixture.Context);
            _orgUserRepository = new OrgUserRepository(_fixture.Context);
        }

        [Fact]
        public async Task GetTasksByOrganizationAsync_OnlyReturnsTasksForOrganization()
        {
            // Arrange
            var org1 = TestDataFactory.CreateOrganization("Org 1");
            var org2 = TestDataFactory.CreateOrganization("Org 2");
            var user1 = TestDataFactory.CreateUser("user1@org1.com");
            var user2 = TestDataFactory.CreateUser("user2@org2.com");

            await _fixture.Context.Organizations.AddAsync(org1);
            await _fixture.Context.Organizations.AddAsync(org2);
            await _fixture.Context.Users.AddAsync(user1);
            await _fixture.Context.Users.AddAsync(user2);

            var task1 = TestDataFactory.CreateTask("Task in Org1", org1.Id, user1.Id);
            var task2 = TestDataFactory.CreateTask("Task in Org2", org2.Id, user2.Id);
            await _fixture.Context.TaskItems.AddAsync(task1);
            await _fixture.Context.TaskItems.AddAsync(task2);
            await _fixture.Context.SaveChangesAsync();

            // Act
            var org1Tasks = await _taskRepository.GetTasksByOrganizationAsync(org1.Id);
            var org2Tasks = await _taskRepository.GetTasksByOrganizationAsync(org2.Id);

            // Assert
            Assert.Single(org1Tasks);
            Assert.Equal("Task in Org1", org1Tasks.First().Title);
            Assert.Single(org2Tasks);
            Assert.Equal("Task in Org2", org2Tasks.First().Title);
        }

        [Fact]
        public async Task GetTasksFilteredAsync_EnforcesOrganizationBoundary()
        {
            // Arrange
            var org1 = TestDataFactory.CreateOrganization("Org 1");
            var org2 = TestDataFactory.CreateOrganization("Org 2");
            var user1 = TestDataFactory.CreateUser("user1@org1.com");
            var user2 = TestDataFactory.CreateUser("user2@org2.com");

            await _fixture.Context.Organizations.AddAsync(org1);
            await _fixture.Context.Organizations.AddAsync(org2);
            await _fixture.Context.Users.AddAsync(user1);
            await _fixture.Context.Users.AddAsync(user2);

            var highPriorityTask1 = TestDataFactory.CreateTask("High Task in Org1", org1.Id, user1.Id, 
                Domain.Enums.TaskStatus.Todo, Domain.Enums.TaskPriority.High);
            var highPriorityTask2 = TestDataFactory.CreateTask("High Task in Org2", org2.Id, user2.Id, 
                Domain.Enums.TaskStatus.Todo, Domain.Enums.TaskPriority.High);

            await _fixture.Context.TaskItems.AddAsync(highPriorityTask1);
            await _fixture.Context.TaskItems.AddAsync(highPriorityTask2);
            await _fixture.Context.SaveChangesAsync();

            // Act - Query Org1 for high priority tasks
            var org1HighTasks = await _taskRepository.GetTasksFilteredAsync(org1.Id, null, Domain.Enums.TaskPriority.High);

            // Assert - Should only get Org1's high priority task
            Assert.Single(org1HighTasks);
            Assert.Equal("High Task in Org1", org1HighTasks.First().Title);
            Assert.Equal(org1.Id, org1HighTasks.First().OrganizationId);
        }

        [Fact]
        public async Task GetUsersByOrganizationAsync_OnlyReturnsUsersInOrganization()
        {
            // Arrange
            var org1 = TestDataFactory.CreateOrganization("Org 1");
            var org2 = TestDataFactory.CreateOrganization("Org 2");
            var user1 = TestDataFactory.CreateUser("user1@org1.com");
            var user2 = TestDataFactory.CreateUser("user2@org2.com");
            var user3 = TestDataFactory.CreateUser("user3@org1.com");

            await _fixture.Context.Organizations.AddAsync(org1);
            await _fixture.Context.Organizations.AddAsync(org2);
            await _fixture.Context.Users.AddAsync(user1);
            await _fixture.Context.Users.AddAsync(user2);
            await _fixture.Context.Users.AddAsync(user3);

            var orgUser1 = TestDataFactory.CreateOrgUser(org1.Id, user1.Id, UserRole.Admin);
            var orgUser2 = TestDataFactory.CreateOrgUser(org2.Id, user2.Id, UserRole.Admin);
            var orgUser3 = TestDataFactory.CreateOrgUser(org1.Id, user3.Id, UserRole.Member);

            await _fixture.Context.OrgUsers.AddAsync(orgUser1);
            await _fixture.Context.OrgUsers.AddAsync(orgUser2);
            await _fixture.Context.OrgUsers.AddAsync(orgUser3);
            await _fixture.Context.SaveChangesAsync();

            // Act
            var org1Users = await _orgUserRepository.GetUsersByOrganizationAsync(org1.Id);
            var org2Users = await _orgUserRepository.GetUsersByOrganizationAsync(org2.Id);

            // Assert
            Assert.Equal(2, org1Users.Count);
            Assert.Single(org2Users);
            Assert.All(org1Users, ou => Assert.Equal(org1.Id, ou.OrganizationId));
            Assert.All(org2Users, ou => Assert.Equal(org2.Id, ou.OrganizationId));
        }

        [Fact]
        public async Task SoftDeleteFilter_ExcludesDeletedTasksAcrossOrganizations()
        {
            // Arrange
            var org1 = TestDataFactory.CreateOrganization("Org 1");
            var org2 = TestDataFactory.CreateOrganization("Org 2");
            var user1 = TestDataFactory.CreateUser("user1@org1.com");
            var user2 = TestDataFactory.CreateUser("user2@org2.com");

            await _fixture.Context.Organizations.AddAsync(org1);
            await _fixture.Context.Organizations.AddAsync(org2);
            await _fixture.Context.Users.AddAsync(user1);
            await _fixture.Context.Users.AddAsync(user2);

            var activeTask1 = TestDataFactory.CreateTask("Active Task in Org1", org1.Id, user1.Id);
            var deletedTask1 = TestDataFactory.CreateTask("Deleted Task in Org1", org1.Id, user1.Id);
            var activeTask2 = TestDataFactory.CreateTask("Active Task in Org2", org2.Id, user2.Id);

            deletedTask1.IsDeleted = true;

            await _fixture.Context.TaskItems.AddAsync(activeTask1);
            await _fixture.Context.TaskItems.AddAsync(deletedTask1);
            await _fixture.Context.TaskItems.AddAsync(activeTask2);
            await _fixture.Context.SaveChangesAsync();

            // Act
            var org1Tasks = await _taskRepository.GetTasksByOrganizationAsync(org1.Id);
            var org2Tasks = await _taskRepository.GetTasksByOrganizationAsync(org2.Id);

            // Assert - Soft-deleted tasks should not be returned
            Assert.Single(org1Tasks);
            Assert.Equal("Active Task in Org1", org1Tasks.First().Title);
            Assert.Single(org2Tasks);
            Assert.Equal("Active Task in Org2", org2Tasks.First().Title);
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
