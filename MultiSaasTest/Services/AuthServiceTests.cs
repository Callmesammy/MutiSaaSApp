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
    /// Unit tests for AuthService - Feature #1: Register Organization
    /// Tests organization registration and login functionality.
    /// </summary>
    public class AuthServiceTests : IDisposable
    {
        private readonly TestDatabaseFixture _fixture;
        private readonly Mock<IPasswordHashService> _passwordHashServiceMock;
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _fixture = new TestDatabaseFixture();
            _passwordHashServiceMock = new Mock<IPasswordHashService>();
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();

            var userRepo = new UserRepository(_fixture.Context);
            var orgRepo = new OrganizationRepository(_fixture.Context);
            var orgUserRepo = new OrgUserRepository(_fixture.Context);

            _authService = new AuthService(
                userRepository: userRepo,
                organizationRepository: orgRepo,
                orgUserRepository: orgUserRepo,
                jwtTokenService: _jwtTokenServiceMock.Object,
                passwordHashService: _passwordHashServiceMock.Object);
        }

        [Fact]
        public async Task RegisterOrganizationAsync_WithValidData_CreatesOrgAndReturnsAuthResponse()
        {
            // Arrange
            var request = new Application.DTOs.Auth.RegisterOrganizationRequest
            {
                OrganizationName = "Acme Corp",
                AdminEmail = "admin@acme.com",
                AdminPassword = "SecureP@ss123"
            };

            _passwordHashServiceMock.Setup(x => x.HashPassword(It.IsAny<string>()))
                .Returns("hashed_password");
            _jwtTokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<UserRole>()))
                .Returns("jwt_token_123");

            // Act
            var result = await _authService.RegisterOrganizationAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Acme Corp", result.OrganizationName);
            Assert.Equal("admin@acme.com", result.Email);
            Assert.Equal("jwt_token_123", result.Token);
            Assert.Equal("Admin", result.Role);
        }

        [Fact]
        public async Task RegisterOrganizationAsync_WithDuplicateOrgName_ThrowsConflictException()
        {
            // Arrange
            var org = TestDataFactory.CreateOrganization("Existing Org");
            await _fixture.Context.Organizations.AddAsync(org);
            await _fixture.Context.SaveChangesAsync();

            var request = new Application.DTOs.Auth.RegisterOrganizationRequest
            {
                OrganizationName = "Existing Org",
                AdminEmail = "admin@test.com",
                AdminPassword = "SecureP@ss123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(async () =>
                await _authService.RegisterOrganizationAsync(request));
        }

        [Fact]
        public async Task RegisterOrganizationAsync_WithDuplicateEmail_ThrowsConflictException()
        {
            // Arrange
            var user = TestDataFactory.CreateUser("admin@test.com");
            await _fixture.Context.Users.AddAsync(user);
            await _fixture.Context.SaveChangesAsync();

            var request = new Application.DTOs.Auth.RegisterOrganizationRequest
            {
                OrganizationName = "New Org",
                AdminEmail = "admin@test.com",
                AdminPassword = "SecureP@ss123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(async () =>
                await _authService.RegisterOrganizationAsync(request));
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var org = TestDataFactory.CreateOrganization();
            var user = TestDataFactory.CreateUser("admin@test.com", "hashed_password");
            await _fixture.Context.Organizations.AddAsync(org);
            await _fixture.Context.Users.AddAsync(user);
            var orgUser = TestDataFactory.CreateOrgUser(org.Id, user.Id, UserRole.Admin);
            await _fixture.Context.OrgUsers.AddAsync(orgUser);
            await _fixture.Context.SaveChangesAsync();

            _passwordHashServiceMock.Setup(x => x.VerifyPassword("password123", "hashed_password"))
                .Returns(true);
            _jwtTokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), 
                It.IsAny<Guid>(), It.IsAny<UserRole>()))
                .Returns("jwt_token_123");

            // Act
            var result = await _authService.LoginAsync("admin@test.com", "password123", org.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("admin@test.com", result.Email);
            Assert.Equal("jwt_token_123", result.Token);
            Assert.Equal("Admin", result.Role);
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }
    }
}
