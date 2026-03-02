using System.Security.Claims;
using Application.Interfaces;

namespace MutiSaaSApp.Middleware
{
    /// <summary>
    /// Middleware for validating user's organization membership on every request.
    /// Extracts organization ID from JWT claims and verifies the user belongs to that organization.
    /// </summary>
    public class OrganizationMembershipMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OrganizationMembershipMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the OrganizationMembershipMiddleware class.
        /// </summary>
        public OrganizationMembershipMiddleware(RequestDelegate next, ILogger<OrganizationMembershipMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to validate organization membership.
        /// Skips validation for public endpoints (auth routes).
        /// </summary>
        public async Task InvokeAsync(HttpContext context, IOrgUserRepository orgUserRepository)
        {
            // Skip validation for auth endpoints
            if (context.Request.Path.StartsWithSegments("/api/auth") || 
                context.Request.Path.StartsWithSegments("/api/invite/accept") ||
                context.Request.Path.StartsWithSegments("/health"))
            {
                await _next(context);
                return;
            }

            // Only validate authenticated requests
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                var orgIdClaim = context.User.FindFirst("org_id");

                if (userIdClaim != null && orgIdClaim != null &&
                    Guid.TryParse(userIdClaim.Value, out var userId) &&
                    Guid.TryParse(orgIdClaim.Value, out var organizationId))
                {
                    // Verify user belongs to the organization
                    var orgUser = await orgUserRepository.GetByOrganizationAndUserAsync(organizationId, userId);
                    
                    if (orgUser == null || orgUser.IsDeleted)
                    {
                        _logger.LogWarning(
                            "Unauthorized access attempt. User {UserId} does not belong to organization {OrgId}",
                            userId, organizationId);
                        
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "User is not a member of this organization."
                        });
                        return;
                    }

                    // Store org membership info in HttpContext items for use in controllers
                    context.Items["OrganizationId"] = organizationId;
                    context.Items["UserId"] = userId;
                    context.Items["UserRole"] = orgUser.Role;
                }
            }

            await _next(context);
        }
    }
}
