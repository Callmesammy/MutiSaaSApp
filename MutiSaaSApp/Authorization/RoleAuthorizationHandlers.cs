using Microsoft.AspNetCore.Authorization;

namespace MutiSaaSApp.Authorization
{
    /// <summary>
    /// Custom authorization requirement for Admin role enforcement.
    /// Ensures only users with Admin role in their organization can access the resource.
    /// </summary>
    public class AdminRoleRequirement : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization handler for the AdminRoleRequirement.
    /// Checks if the user has the Admin role claim.
    /// </summary>
    public class AdminRoleHandler : AuthorizationHandler<AdminRoleRequirement>
    {
        /// <summary>
        /// Handles the authorization for admin-only operations.
        /// </summary>
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AdminRoleRequirement requirement)
        {
            // Check if user has the role claim
            var roleClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role);
            
            if (roleClaim != null && roleClaim.Value == "Admin")
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Custom authorization requirement for Member or Admin role enforcement.
    /// Ensures any authenticated user in the organization can access the resource.
    /// </summary>
    public class MemberOrAdminRequirement : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Authorization handler for the MemberOrAdminRequirement.
    /// Checks if the user has either Member or Admin role claim.
    /// </summary>
    public class MemberOrAdminHandler : AuthorizationHandler<MemberOrAdminRequirement>
    {
        /// <summary>
        /// Handles the authorization for member or admin operations.
        /// </summary>
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MemberOrAdminRequirement requirement)
        {
            // Check if user has the role claim with Member or Admin value
            var roleClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role);
            
            if (roleClaim != null && (roleClaim.Value == "Admin" || roleClaim.Value == "Member"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
