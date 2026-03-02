using Application.DTOs.Auth;
using Application.DTOs.Invite;
using Application.DTOs.Task;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infastructure.Repositories;
using Infastructure.Services;
using Moq;
using MultiSaasTest.Fixtures;

namespace MultiSaasTest.Integration
{
    /// <summary>
    /// Integration tests for Auth endpoints - Feature #1
    /// Tests the complete auth flow through the service layer.
    /// </summary>
    public class AuthIntegrationTests : IDisposable
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly Mock<IPasswordHashService> _passwordHashServiceMock;
        private readonly AuthService _authService;

        public AuthIntegrationTests()
        {
            _fixture = new TestDatabaseFixture();
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();
            _passwordHashServiceMock = new Mock<IPasswordHashService>();

            var userRepo = new UserRepository(_fixture.Context);
            var orgRepo = new OrganizationRepository(_fixture.Context);
            var orgUserRepo = new OrgUserRepository(_fixture.Context);

            _authService = new AuthService(userRepo, orgRepo, orgUserRepo, _jwtTokenServiceMock.Object, _passwordHashServiceMock.Object);
        }

        [Fact]
        public async Task RegisterAndLoginFlow_CreatesOrgAndAuthenticatesUser()
        {
            // Setup mocks
            _passwordHashServiceMock.Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashed_password");
            _passwordHashServiceMock.Setup(x => x.VerifyPassword("TestP@ss123", "hashed_password"))
                .Returns(true);
            _jwtTokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), UserRole.Admin))
                .Returns("jwt_token_123");

            // Step 1: Register organization
            var registerRequest = new RegisterOrganizationRequest
            {
                OrganizationName = "TechCorp",
                AdminEmail = "admin@techcorp.com",
                AdminPassword = "TestP@ss123"
            };

            var registerResult = await _authService.RegisterOrganizationAsync(registerRequest);

            Assert.NotNull(registerResult);
            Assert.Equal("TechCorp", registerResult.OrganizationName);
            Assert.Equal("admin@techcorp.com", registerResult.Email);

            // Step 2: Login with registered user
            var org = await _fixture.Context.Organizations.FindAsync(registerResult.OrganizationId);
            var loginResult = await _authService.LoginAsync("admin@techcorp.com", "TestP@ss123", org.Id);

            Assert.NotNull(loginResult);
            Assert.Equal("admin@techcorp.com", loginResult.Email);
            Assert.True(loginResult.Role == Domain.Enums.UserRole.Admin);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ThrowsUnauthorizedException()
        {
            // Arrange
            var org = TestDataFactory.CreateOrganization();
            var user = TestDataFactory.CreateUser("admin@test.com", "hashed_password");
            await _fixture.Context.Organizations.AddAsync(org);
            await _fixture.Context.Users.AddAsync(user);
            var orgUser = TestDataFactory.CreateOrgUser(org.Id, user.Id, UserRole.Admin);
            await _fixture.Context.OrgUsers.AddAsync(orgUser);
            await _fixture.Context.SaveChangesAsync();

            _passwordHashServiceMock.Setup(x => x.VerifyPassword("wrong_password", "hashed_password"))
                .Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _authService.LoginAsync("admin@test.com", "wrong_password", org.Id));
        }

        [Fact]
        public async Task Login_WithNonMemberUser_ThrowsUnauthorizedException()
        {
            // Arrange - User exists but not in organization
            var org1 = TestDataFactory.CreateOrganization();
            var org2 = TestDataFactory.CreateOrganization("Another Org");
            var user = TestDataFactory.CreateUser("user@test.com", "hashed_password");

            await _fixture.Context.Organizations.AddAsync(org1);
            await _fixture.Context.Organizations.AddAsync(org2);
            await _fixture.Context.Users.AddAsync(user);

            // User only in org2, try to login to org1
            var orgUser = TestDataFactory.CreateOrgUser(org2.Id, user.Id, UserRole.Member);
            await _fixture.Context.OrgUsers.AddAsync(orgUser);
            await _fixture.Context.SaveChangesAsync();

            _passwordHashServiceMock.Setup(x => x.VerifyPassword("password", "hashed_password"))
                .Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _authService.LoginAsync("user@test.com", "password", org1.Id));
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
