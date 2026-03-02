using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infastructure.Repositories;
using Infastructure.Services;
using Moq;
using MultiSaasTest.Fixtures;

namespace MultiSaasTest.Services
{
    /// <summary>
    /// Unit tests for InviteService - Feature #2: Invite Users to Organization
    /// Tests invite token creation, validation, and acceptance.
    /// </summary>
    public class InviteServiceTests : IDisposable
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly Mock<ITokenGeneratorService> _tokenGeneratorMock;
        private readonly Mock<IPasswordHashService> _passwordHashServiceMock;
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly InviteService _inviteService;

        public InviteServiceTests()
        {
            _fixture = new TestDatabaseFixture();
            _tokenGeneratorMock = new Mock<ITokenGeneratorService>();
            _passwordHashServiceMock = new Mock<IPasswordHashService>();
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();

            var inviteRepo = new InviteTokenRepository(_fixture.Context);
            var userRepo = new UserRepository(_fixture.Context);
            var orgRepo = new OrganizationRepository(_fixture.Context);
            var orgUserRepo = new OrgUserRepository(_fixture.Context);

            _inviteService = new InviteService(
                inviteTokenRepository: inviteRepo,
                userRepository: userRepo,
                organizationRepository: orgRepo,
                orgUserRepository: orgUserRepo,
                tokenGeneratorService: _tokenGeneratorMock.Object,
                passwordHashService: _passwordHashServiceMock.Object,
                jwtTokenService: _jwtTokenServiceMock.Object);
        }

        [Fact]
        public async Task CreateInviteAsync_WithValidAdmin_CreatesTokenAndReturnsInviteResponse()
        {
            // Arrange
            var org = TestDataFactory.CreateOrganization();
            var admin = TestDataFactory.CreateUser("admin@test.com");
            await _fixture.Context.Organizations.AddAsync(org);
            await _fixture.Context.Users.AddAsync(admin);
            var orgUser = TestDataFactory.CreateOrgUser(org.Id, admin.Id, UserRole.Admin);
            await _fixture.Context.OrgUsers.AddAsync(orgUser);
            await _fixture.Context.SaveChangesAsync();

            var request = new Application.DTOs.Invite.CreateInviteRequest { Email = "newuser@test.com" };
            _tokenGeneratorMock.Setup(x => x.GenerateSecureToken(It.IsAny<int>()))
                .Returns("generated_token_123");

            // Act
            var result = await _inviteService.CreateInviteAsync(org.Id, admin.Id, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("newuser@test.com", result.Email);
            Assert.Equal("generated_token_123", result.Token);
            Assert.Equal(org.Id, result.OrganizationId);
        }

        [Fact]
        public async Task CreateInviteAsync_WithNonAdminUser_ThrowsUnauthorizedException()
        {
            // Arrange
            var org = TestDataFactory.CreateOrganization();
            var member = TestDataFactory.CreateUser("member@test.com");
            await _fixture.Context.Organizations.AddAsync(org);
            await _fixture.Context.Users.AddAsync(member);
            var orgUser = TestDataFactory.CreateOrgUser(org.Id, member.Id, UserRole.Member);
            await _fixture.Context.OrgUsers.AddAsync(orgUser);
            await _fixture.Context.SaveChangesAsync();

            var request = new Application.DTOs.Invite.CreateInviteRequest { Email = "newuser@test.com" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _inviteService.CreateInviteAsync(org.Id, member.Id, request));
        }

        [Fact]
        public async Task AcceptInviteAsync_WithValidToken_CreatesUserAndReturnsAuthResponse()
        {
            // Arrange
            var org = TestDataFactory.CreateOrganization();
            var admin = TestDataFactory.CreateUser("admin@test.com");
            await _fixture.Context.Organizations.AddAsync(org);
            await _fixture.Context.Users.AddAsync(admin);
            var orgUser = TestDataFactory.CreateOrgUser(org.Id, admin.Id, UserRole.Admin);
            await _fixture.Context.OrgUsers.AddAsync(orgUser);

            var inviteToken = TestDataFactory.CreateInviteToken(org.Id, "newuser@test.com", "token_123", admin.Id);
            await _fixture.Context.InviteTokens.AddAsync(inviteToken);
            await _fixture.Context.SaveChangesAsync();

            var request = new Application.DTOs.Invite.AcceptInviteRequest
            {
                Token = "token_123",
                Password = "SecureP@ss123",
                FirstName = "Jane",
                LastName = "Doe"
            };

            _passwordHashServiceMock.Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashed_password");
            _jwtTokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<UserRole>()))
                .Returns("jwt_token_123");

            // Act
            var result = await _inviteService.AcceptInviteAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("newuser@test.com", result.Email);
            Assert.Equal(org.Id, result.OrganizationId);
            Assert.Equal("jwt_token_123", result.Token);
        }

        [Fact]
        public async Task AcceptInviteAsync_WithExpiredToken_ThrowsConflictException()
        {
            // Arrange
            var org = TestDataFactory.CreateOrganization();
            var admin = TestDataFactory.CreateUser("admin@test.com");
            await _fixture.Context.Organizations.AddAsync(org);
            await _fixture.Context.Users.AddAsync(admin);

            var expiredToken = new InviteToken
            {
                OrganizationId = org.Id,
                Email = "expired@test.com",
                Token = "expired_token_123",
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
                IsUsed = false,
                InvitedByUserId = admin.Id
            };
            await _fixture.Context.InviteTokens.AddAsync(expiredToken);
            await _fixture.Context.SaveChangesAsync();

            var request = new Application.DTOs.Invite.AcceptInviteRequest
            {
                Token = "expired_token_123",
                Password = "SecureP@ss123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(async () =>
                await _inviteService.AcceptInviteAsync(request));
        }

        [Fact]
        public async Task AcceptInviteAsync_WithAlreadyUsedToken_ThrowsConflictException()
        {
            // Arrange
            var org = TestDataFactory.CreateOrganization();
            var admin = TestDataFactory.CreateUser("admin@test.com");
            var existingUser = TestDataFactory.CreateUser("existing@test.com");
            await _fixture.Context.Organizations.AddAsync(org);
            await _fixture.Context.Users.AddAsync(admin);
            await _fixture.Context.Users.AddAsync(existingUser);

            var usedToken = new InviteToken
            {
                OrganizationId = org.Id,
                Email = "used@test.com",
                Token = "used_token_123",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsUsed = true,
                AcceptedAt = DateTime.UtcNow,
                AcceptedByUserId = existingUser.Id,
                InvitedByUserId = admin.Id
            };
            await _fixture.Context.InviteTokens.AddAsync(usedToken);
            await _fixture.Context.SaveChangesAsync();

            var request = new Application.DTOs.Invite.AcceptInviteRequest
            {
                Token = "used_token_123",
                Password = "SecureP@ss123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(async () =>
                await _inviteService.AcceptInviteAsync(request));
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
