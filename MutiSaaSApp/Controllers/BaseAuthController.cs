using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MutiSaaSApp.Controllers
{
    /// <summary>
    /// Base controller for all API controllers.
    /// Provides common authorization and context extraction helpers.
    /// All controllers requiring authentication should inherit from this.
    /// </summary>
    [ApiController]
    [Authorize]
    public abstract class BaseAuthController : ControllerBase
    {
        /// <summary>
        /// Gets the current user's ID from JWT claims.
        /// </summary>
        protected Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User ID could not be determined from claims.");
        }

        /// <summary>
        /// Gets the current user's organization ID from JWT claims.
        /// </summary>
        protected Guid GetOrganizationId()
        {
            var orgIdClaim = User.FindFirst("org_id");
            if (orgIdClaim != null && Guid.TryParse(orgIdClaim.Value, out var organizationId))
            {
                return organizationId;
            }

            throw new UnauthorizedAccessException("Organization ID could not be determined from claims.");
        }

        /// <summary>
        /// Gets the current user's role from JWT claims.
        /// </summary>
        protected UserRole GetUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            if (roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, out var role))
            {
                return role;
            }

            throw new UnauthorizedAccessException("User role could not be determined from claims.");
        }

        /// <summary>
        /// Gets the current user's email from JWT claims.
        /// </summary>
        protected string GetUserEmail()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email);
            if (emailClaim != null)
            {
                return emailClaim.Value;
            }

            throw new UnauthorizedAccessException("User email could not be determined from claims.");
        }

        /// <summary>
        /// Checks if the current user is an admin in their organization.
        /// </summary>
        protected bool IsAdmin()
        {
            try
            {
                return GetUserRole() == UserRole.Admin;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the current user is a member or admin in their organization.
        /// </summary>
        protected bool IsMemberOrAdmin()
        {
            try
            {
                var role = GetUserRole();
                return role == UserRole.Admin || role == UserRole.Member;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifies that the current user is an admin. Throws UnauthorizedAccessException if not.
        /// </summary>
        protected void RequireAdmin()
        {
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("This operation requires admin role.");
            }
        }

        /// <summary>
        /// Verifies that the current user is a member or admin. Throws UnauthorizedAccessException if not.
        /// </summary>
        protected void RequireMemberOrAdmin()
        {
            if (!IsMemberOrAdmin())
            {
                throw new UnauthorizedAccessException("This operation requires member or admin role.");
            }
        }
    }
}
