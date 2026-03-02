namespace Application.Constants
{
    /// <summary>
    /// Contains authorization policy names used throughout the application.
    /// </summary>
    public static class AuthorizationPolicies
    {
        /// <summary>
        /// Policy name for admin-only operations.
        /// Only users with Admin role within the organization can access.
        /// </summary>
        public const string AdminOnly = "AdminOnly";

        /// <summary>
        /// Policy name for operations accessible to both admins and members.
        /// Any authenticated user in the organization can access.
        /// </summary>
        public const string MemberOrAdmin = "MemberOrAdmin";
    }
}
