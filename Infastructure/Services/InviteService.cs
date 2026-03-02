using Application.DTOs.Auth;
using Application.DTOs.Invite;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infastructure.Services;

namespace Infastructure.Services
{
    /// <summary>
    /// Service for handling invitation operations.
    /// Manages creating, validating, and accepting organization invites.
    /// </summary>
    public class InviteService : IInviteService
    {
        private readonly IInviteTokenRepository _inviteTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrgUserRepository _orgUserRepository;
        private readonly ITokenGeneratorService _tokenGeneratorService;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IJwtTokenService _jwtTokenService;

        /// <summary>
        /// Initializes a new instance of the InviteService class.
        /// </summary>
        public InviteService(
            IInviteTokenRepository inviteTokenRepository,
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            IOrgUserRepository orgUserRepository,
            ITokenGeneratorService tokenGeneratorService,
            IPasswordHashService passwordHashService,
            IJwtTokenService jwtTokenService)
        {
            _inviteTokenRepository = inviteTokenRepository;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _orgUserRepository = orgUserRepository;
            _tokenGeneratorService = tokenGeneratorService;
            _passwordHashService = passwordHashService;
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Creates an invite token for a user to join an organization.
        /// Only admins can create invites. Token expires in 48 hours and is single-use.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="adminUserId">The ID of the admin creating the invite.</param>
        /// <param name="request">The invite request with email to invite.</param>
        /// <returns>An InviteResponse with the token and expiry info.</returns>
        public async Task<InviteResponse> CreateInviteAsync(Guid organizationId, Guid adminUserId, CreateInviteRequest request)
        {
            // Verify organization exists
            var organization = await _organizationRepository.GetByIdAsync(organizationId);
            if (organization == null)
            {
                throw new NotFoundException("Organization", organizationId);
            }

            // Verify admin user exists and belongs to the organization
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin == null)
            {
                throw new NotFoundException("User", adminUserId);
            }

            var adminOrgUser = await _orgUserRepository.GetByOrganizationAndUserAsync(organizationId, adminUserId);
            if (adminOrgUser == null)
            {
                throw new UnauthorizedException("User does not belong to this organization.");
            }

            // Verify admin has permission
            if (adminOrgUser.Role != UserRole.Admin)
            {
                throw new UnauthorizedException("Only admins can send invites.");
            }

            // Check if user already exists in the system
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                // Check if user already belongs to this organization
                var existingOrgUser = await _orgUserRepository.GetByOrganizationAndUserAsync(organizationId, existingUser.Id);
                if (existingOrgUser != null)
                {
                    throw new ConflictException($"User with email '{request.Email}' is already a member of this organization.");
                }
            }

            // Generate secure token
            var token = _tokenGeneratorService.GenerateSecureToken();
            var expiresAt = DateTime.UtcNow.AddHours(48);

            // Create invite token
            var inviteToken = new InviteToken(
                token: token,
                email: request.Email,
                organizationId: organizationId,
                invitedByUserId: adminUserId,
                expiryHours: 48);

            await _inviteTokenRepository.AddAsync(inviteToken);
            await _inviteTokenRepository.SaveChangesAsync();

            return new InviteResponse
            {
                InviteTokenId = inviteToken.Id,
                Email = inviteToken.Email,
                OrganizationId = organizationId,
                Token = token,
                ExpiresAt = expiresAt,
                Message = $"Invitation sent to {request.Email}. They have 48 hours to accept."
            };
        }

        /// <summary>
        /// Accepts an invite token and creates a user account or adds existing user to organization.
        /// Token must be valid (not expired and not used).
        /// </summary>
        /// <param name="request">The accept invite request with token, password, and optional name fields.</param>
        /// <returns>An AuthResponse with user info and JWT token.</returns>
        public async Task<AuthResponse> AcceptInviteAsync(AcceptInviteRequest request)
        {
            // Get the invite token
            var inviteToken = await _inviteTokenRepository.GetByTokenAsync(request.Token);
            if (inviteToken == null)
            {
                throw new NotFoundException("Invite token not found or is invalid.");
            }

            // Validate token is still valid
            if (!inviteToken.IsValid())
            {
                if (inviteToken.IsExpired())
                {
                    throw new Domain.Exceptions.ValidationException(
                        "Token",
                        "Invite token has expired. Request a new invitation.");
                }

                if (inviteToken.IsUsed)
                {
                    throw new Domain.Exceptions.ValidationException(
                        "Token",
                        "Invite token has already been used.");
                }
            }

            // Check if user already exists
            var user = await _userRepository.GetByEmailAsync(inviteToken.Email);

            if (user == null)
            {
                // Create new user
                var passwordHash = _passwordHashService.HashPassword(request.Password);
                user = new User(inviteToken.Email, passwordHash)
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
            }
            else
            {
                // Verify user doesn't already belong to this organization
                var existingOrgUser = await _orgUserRepository.GetByOrganizationAndUserAsync(
                    inviteToken.OrganizationId, user.Id);

                if (existingOrgUser != null)
                {
                    throw new ConflictException("User is already a member of this organization.");
                }
            }

            // Create OrgUser relationship with Member role
            var orgUser = new OrgUser(inviteToken.OrganizationId, user.Id, UserRole.Member);
            await _orgUserRepository.AddAsync(orgUser);

            // Mark invite token as used
            inviteToken.Accept(user.Id);
            await _inviteTokenRepository.UpdateAsync(inviteToken);

            await _orgUserRepository.SaveChangesAsync();

            // Get organization for response
            var organization = await _organizationRepository.GetByIdAsync(inviteToken.OrganizationId);
            if (organization == null)
            {
                throw new NotFoundException("Organization", inviteToken.OrganizationId);
            }

            // Generate JWT token
            var jwtToken = _jwtTokenService.GenerateToken(user.Id, user.Email, inviteToken.OrganizationId, UserRole.Member);

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                OrganizationId = organization.Id,
                OrganizationName = organization.Name,
                Token = jwtToken,
                Role = UserRole.Member.ToString()
            };
        }

        /// <summary>
        /// Retrieves an invite token by its token string.
        /// </summary>
        /// <param name="token">The token string to look up.</param>
        public async Task<InviteToken?> GetInviteTokenAsync(string token)
        {
            return await _inviteTokenRepository.GetByTokenAsync(token);
        }

        /// <summary>
        /// Checks if an invite token is still valid (not expired and not used).
        /// </summary>
        /// <param name="token">The token string to validate.</param>
        public async Task<bool> IsTokenValidAsync(string token)
        {
            var inviteToken = await _inviteTokenRepository.GetByTokenAsync(token);
            if (inviteToken == null)
            {
                return false;
            }

            return inviteToken.IsValid();
        }
    }
}
