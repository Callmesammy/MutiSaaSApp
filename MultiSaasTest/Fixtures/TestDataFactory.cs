using Domain.Entities;
using Domain.Enums;

namespace MultiSaasTest.Fixtures
{
    /// <summary>
    /// Factory for creating test data objects.
    /// </summary>
    public static class TestDataFactory
    {
        public static Organization CreateOrganization(string name = "Test Org", string? description = null)
        {
            return new Organization { Name = name, Description = description };
        }

        public static User CreateUser(string email = "test@example.com", string passwordHash = "hashed_password", 
            string firstName = "John", string lastName = "Doe")
        {
            return new User 
            { 
                Email = email, 
                PasswordHash = passwordHash,
                FirstName = firstName,
                LastName = lastName
            };
        }

        public static OrgUser CreateOrgUser(Guid organizationId, Guid userId, UserRole role = UserRole.Member)
        {
            return new OrgUser 
            { 
                OrganizationId = organizationId, 
                UserId = userId, 
                Role = role 
            };
        }

        public static InviteToken CreateInviteToken(Guid organizationId, string email = "invited@example.com", 
            string token = "test_token_123", Guid? invitedByUserId = null)
        {
            return new InviteToken 
            { 
                OrganizationId = organizationId, 
                Email = email, 
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(2),
                IsUsed = false,
                InvitedByUserId = invitedByUserId ?? Guid.NewGuid()
            };
        }

        public static TaskItem CreateTask(string title = "Test Task", Guid organizationId = default, 
            Guid createdByUserId = default, Domain.Enums.TaskStatus status = Domain.Enums.TaskStatus.Todo,
            Domain.Enums.TaskPriority priority = Domain.Enums.TaskPriority.Medium)
        {
            if (organizationId == default)
                organizationId = Guid.NewGuid();
            if (createdByUserId == default)
                createdByUserId = Guid.NewGuid();

            return new TaskItem(
                title: title,
                organizationId: organizationId,
                createdByUserId: createdByUserId,
                status: status,
                priority: priority)
            {
                Description = "Test task description"
            };
        }
    }
}
