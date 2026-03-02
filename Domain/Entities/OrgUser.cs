using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    /// <summary>
    /// Represents the relationship between a User and an Organization.
    /// Tracks the role the user has within the organization.
    /// </summary>
    public class OrgUser : BaseEntity
    {
        /// <summary>
        /// Gets or sets the ID of the organization.
        /// </summary>
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the role of the user within this organization.
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Navigation property: the organization.
        /// </summary>
        public Organization? Organization { get; set; }

        /// <summary>
        /// Navigation property: the user.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Initializes a new instance of the OrgUser class.
        /// </summary>
        public OrgUser() : base()
        {
        }

        /// <summary>
        /// Creates a new OrgUser relationship with the specified organization, user, and role.
        /// </summary>
        /// <param name="organizationId">The ID of the organization.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="role">The role of the user within the organization.</param>
        public OrgUser(Guid organizationId, Guid userId, UserRole role) : base()
        {
            OrganizationId = organizationId;
            UserId = userId;
            Role = role;
        }
    }
}
