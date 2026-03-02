namespace Domain.Enums
{
    /// <summary>
    /// Defines the roles a user can have within an organization.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Admin role - full permissions for the organization.
        /// </summary>
        Admin = 1,

        /// <summary>
        /// Member role - limited permissions within the organization.
        /// </summary>
        Member = 2
    }
}
