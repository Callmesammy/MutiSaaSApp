using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MutiSaaSApp.Common;

namespace MutiSaaSApp.Controllers
{
    /// <summary>
    /// Controller for authentication-related endpoints.
    /// Handles user registration, login, and organization creation.
    /// Public endpoints - no authorization required.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterOrganizationRequest> _registerValidator;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes a new instance of the AuthController class.
        /// </summary>
        public AuthController(
            IAuthService authService,
            IValidator<RegisterOrganizationRequest> registerValidator,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new organization and creates an admin user.
        /// </summary>
        /// <param name="request">The registration request.</param>
        /// <returns>
        /// 200 OK: Registration successful with JWT token.
        /// 400 Bad Request: Validation failed.
        /// 409 Conflict: Organization or email already exists.
        /// </returns>
        [HttpPost("register-organization")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterOrganization([FromBody] RegisterOrganizationRequest request)
        {
            _logger.LogInformation("Register organization request received for: {Email}", request.AdminEmail);

            // Validate request
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => string.Join(", ", g.Select(e => e.ErrorMessage)));

                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed.", errors));
            }

            try
            {
                var result = await _authService.RegisterOrganizationAsync(request);
                _logger.LogInformation("Organization registered successfully for: {Email}", request.AdminEmail);
                return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Organization registered successfully."));
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning("Conflict during registration: {Message}", ex.Message);
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during organization registration");
                throw;
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="organizationId">The organization ID.</param>
        /// <returns>
        /// 200 OK: Login successful with JWT token.
        /// 401 Unauthorized: Invalid credentials or user not in organization.
        /// 404 Not Found: Organization not found.
        /// </returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Login(
            [FromQuery] string email,
            [FromQuery] string password,
            [FromQuery] Guid organizationId)
        {
            _logger.LogInformation("Login request for: {Email}, Organization: {OrgId}", email, organizationId);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Email and password are required."));
            }

            try
            {
                var result = await _authService.LoginAsync(email, password, organizationId);
                _logger.LogInformation("Login successful for: {Email}", email);
                return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful."));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning("Unauthorized login attempt: {Message}", ex.Message);
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Not found during login: {Message}", ex.Message);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                throw;
            }
        }
    }
}
