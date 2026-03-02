using Application.Constants;
using Application.DTOs.Auth;
using Application.DTOs.Invite;
using Application.Interfaces;
using Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MutiSaaSApp.Common;
using System.Security.Claims;

namespace MutiSaaSApp.Controllers
{
    /// <summary>
    /// Controller for organization invite-related endpoints.
    /// Handles creating invites and accepting them to join organizations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InviteController : BaseAuthController
    {
        private readonly IInviteService _inviteService;
        private readonly IValidator<CreateInviteRequest> _createInviteValidator;
        private readonly IValidator<AcceptInviteRequest> _acceptInviteValidator;
        private readonly ILogger<InviteController> _logger;

        /// <summary>
        /// Initializes a new instance of the InviteController class.
        /// </summary>
        public InviteController(
            IInviteService inviteService,
            IValidator<CreateInviteRequest> createInviteValidator,
            IValidator<AcceptInviteRequest> acceptInviteValidator,
            ILogger<InviteController> logger)
        {
            _inviteService = inviteService;
            _createInviteValidator = createInviteValidator;
            _acceptInviteValidator = acceptInviteValidator;
            _logger = logger;
        }

        /// <summary>
        /// Creates an invite token for a user to join an organization.
        /// Only admins can create invites.
        /// </summary>
        /// <param name="organizationId">The organization ID.</param>
        /// <param name="request">The invite request containing the email to invite.</param>
        /// <returns>
        /// 200 OK: Invite created successfully with token.
        /// 400 Bad Request: Validation failed.
        /// 401 Unauthorized: User is not authorized or not an admin.
        /// 404 Not Found: Organization or user not found.
        /// 409 Conflict: User already in organization.
        /// </returns>
        [HttpPost("organizations/{organizationId}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
        [ProducesResponseType(typeof(ApiResponse<InviteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateInvite(
            [FromRoute] Guid organizationId,
            [FromBody] CreateInviteRequest request)
        {
            _logger.LogInformation("Create invite request for organization {OrgId} to email {Email}", organizationId, request.Email);

            // Get user ID from JWT claims using helper method
            var userId = GetUserId();

            // Validate request
            var validationResult = await _createInviteValidator.ValidateAsync(request);
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
                var result = await _inviteService.CreateInviteAsync(organizationId, userId, request);
                _logger.LogInformation("Invite created successfully for {Email} to organization {OrgId}", request.Email, organizationId);
                return Ok(ApiResponse<InviteResponse>.SuccessResponse(result, "Invite created successfully."));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Not found during invite creation: {Message}", ex.Message);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning("Unauthorized invite creation attempt: {Message}", ex.Message);
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning("Conflict during invite creation: {Message}", ex.Message);
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during invite creation");
                throw;
            }
        }

        /// <summary>
        /// Accepts an invite token and adds the user to the organization.
        /// Creates a new user account if one doesn't exist with that email.
        /// </summary>
        /// <param name="request">The accept invite request with token and password.</param>
        /// <returns>
        /// 200 OK: Invite accepted successfully, user logged in with JWT token.
        /// 400 Bad Request: Validation failed.
        /// 404 Not Found: Invite token not found or invalid.
        /// 410 Gone: Invite token has expired.
        /// 409 Conflict: User already in organization.
        /// </returns>
        [HttpPost("accept")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
        {
            _logger.LogInformation("Accept invite request received with token starting with: {TokenStart}", 
                request.Token.Substring(0, Math.Min(10, request.Token.Length)));

            // Validate request
            var validationResult = await _acceptInviteValidator.ValidateAsync(request);
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
                var result = await _inviteService.AcceptInviteAsync(request);
                _logger.LogInformation("Invite accepted successfully for user {UserId}", result.UserId);
                return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Invite accepted successfully."));
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Invite token not found: {Message}", ex.Message);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Domain.Exceptions.ValidationException ex)
            {
                _logger.LogWarning("Validation error during invite acceptance: {Message}", ex.Message);

                // Return 410 Gone for expired tokens
                if (ex.Errors.ContainsKey("Token") && ex.Errors["Token"].Contains("expired"))
                {
                    return StatusCode(StatusCodes.Status410Gone, 
                        ApiResponse<object>.ErrorResponse(ex.Message, ex.Errors));
                }

                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.Errors));
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning("Conflict during invite acceptance: {Message}", ex.Message);
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during invite acceptance");
                throw;
            }
        }
    }
}
