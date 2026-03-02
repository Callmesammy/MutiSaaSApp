namespace Application.Constants
{
    /// <summary>
    /// Cache key strategy for distributed caching.
    /// Defines consistent key patterns for all cached data types.
    /// </summary>
    public static class CacheKeys
    {
        // Cache key prefixes
        private const string TaskPrefix = "task";
        private const string UserPrefix = "user";
        private const string OrgPrefix = "org";
        private const string InvitePrefix = "invite";
        private const string OrgTasksPrefix = "org_tasks";

        // Task cache keys
        /// <summary>
        /// Key for a single task: task:{organizationId}:{taskId}
        /// </summary>
        public static string GetTaskKey(Guid organizationId, Guid taskId) =>
            $"{TaskPrefix}:{organizationId:N}:{taskId:N}";

        /// <summary>
        /// Key for all tasks in an organization: org_tasks:{organizationId}
        /// </summary>
        public static string GetOrgTasksKey(Guid organizationId) =>
            $"{OrgTasksPrefix}:{organizationId:N}";

        /// <summary>
        /// Key for filtered tasks: org_tasks:{organizationId}:{status}:{priority}
        /// </summary>
        public static string GetOrgTasksFilteredKey(Guid organizationId, string? status = null, string? priority = null) =>
            $"{OrgTasksPrefix}:{organizationId:N}:{status ?? "all"}:{priority ?? "all"}";

        // User cache keys
        /// <summary>
        /// Key for a single user: user:{userId}
        /// </summary>
        public static string GetUserKey(Guid userId) =>
            $"{UserPrefix}:{userId:N}";

        /// <summary>
        /// Key for user by email: user:email:{email}
        /// </summary>
        public static string GetUserByEmailKey(string email) =>
            $"{UserPrefix}:email:{email}";

        // Organization cache keys
        /// <summary>
        /// Key for a single organization: org:{organizationId}
        /// </summary>
        public static string GetOrgKey(Guid organizationId) =>
            $"{OrgPrefix}:{organizationId:N}";

        /// <summary>
        /// Key for organization by name: org:name:{organizationName}
        /// </summary>
        public static string GetOrgByNameKey(string organizationName) =>
            $"{OrgPrefix}:name:{organizationName}";

        // Invite cache keys
        /// <summary>
        /// Key for an invite token: invite:{token}
        /// </summary>
        public static string GetInviteTokenKey(string token) =>
            $"{InvitePrefix}:{token}";

        /// <summary>
        /// Key for invites by email: invite:email:{email}
        /// </summary>
        public static string GetInvitesByEmailKey(string email) =>
            $"{InvitePrefix}:email:{email}";

        /// <summary>
        /// Key for invites by organization: invite:org:{organizationId}
        /// </summary>
        public static string GetInvitesByOrgKey(Guid organizationId) =>
            $"{InvitePrefix}:org:{organizationId:N}";

        /// <summary>
        /// Invalidates all task-related cache for an organization.
        /// Call this when tasks are created, updated, or deleted.
        /// </summary>
        public static string[] GetTaskCacheKeysToInvalidate(Guid organizationId) =>
            new[]
            {
                $"{OrgTasksPrefix}:{organizationId:N}:*",
            };
    }
}
