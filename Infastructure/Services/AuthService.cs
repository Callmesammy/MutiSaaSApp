using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infastructure.Services;

namespace Infastructure.Services
{
    /// <summary>
    /// Service for handling authentication operations.
    /// Implements user registration, organization creation, and login logic.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrgUserRepository _orgUserRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordHashService _passwordHashService;

        /// <summary>
        /// Initializes a new instance of the AuthService class.
        /// </summary>
        public AuthService(
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            IOrgUserRepository orgUserRepository,
            IJwtTokenService jwtTokenService,
            IPasswordHashService passwordHashService)
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _orgUserRepository = orgUserRepository;
            _jwtTokenService = jwtTokenService;
            _passwordHashService = passwordHashService;
        }

        /// <summary>
        /// Registers a new organization and creates an admin user.
        /// The user is automatically assigned the Admin role in the new organization.
        /// </summary>
        /// <param name="request">Registration request with organization name, email, and password.</param>
        /// <returns>Authentication response with JWT token.</returns>
        public async Task<AuthResponse> RegisterOrganizationAsync(RegisterOrganizationRequest request)
        {
            // Check if organization name already exists
            if (await _organizationRepository.NameExistsAsync(request.OrganizationName))
            {
                throw new ConflictException($"Organization with name '{request.OrganizationName}' already exists.");
            }

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(request.AdminEmail))
            {
                throw new ConflictException($"User with email '{request.AdminEmail}' already exists.");
            }

            // Create the organization
            var organization = new Organization(request.OrganizationName);
            await _organizationRepository.AddAsync(organization);
            await _organizationRepository.SaveChangesAsync();

            // Hash the password
            var passwordHash = _passwordHashService.HashPassword(request.AdminPassword);

            // Create the user
            var user = new User(request.AdminEmail, passwordHash);
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Create the OrgUser relationship with Admin role
            var orgUser = new OrgUser(organization.Id, user.Id, UserRole.Admin);
            await _orgUserRepository.AddAsync(orgUser);
            await _orgUserRepository.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user.Id, user.Email, organization.Id, UserRole.Admin);

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                OrganizationId = organization.Id,
                OrganizationName = organization.Name,
                Token = token,
                Role = UserRole.Admin.ToString()
            };
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// Validates email and password, then generates a token with the user's role in the organization.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="organizationId">The organization ID for context.</param>
        /// <returns>Authentication response with JWT token.</returns>
        public async Task<AuthResponse> LoginAsync(string email, string password, Guid organizationId)
        {
            // Find the user by email
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            // Verify password
            if (!_passwordHashService.VerifyPassword(password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            // Get the user's role in the organization
            var orgUser = await _orgUserRepository.GetByOrganizationAndUserAsync(organizationId, user.Id);
            if (orgUser == null)
            {
                throw new UnauthorizedException($"User does not belong to organization with ID '{organizationId}'.");
            }

            // Get the organization
            var organization = await _organizationRepository.GetByIdAsync(organizationId);
            if (organization == null)
            {
                throw new NotFoundException("Organization", organizationId);
            }

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user.Id, user.Email, organizationId, orgUser.Role);

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                OrganizationId = organization.Id,
                OrganizationName = organization.Name,
                Token = token,
                Role = orgUser.Role.ToString()
            };
        }
    }
}
