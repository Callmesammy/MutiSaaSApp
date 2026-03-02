using Domain.Common;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a tenant organization in the multi-tenant system.
    /// Each organization is isolated and has its own users and resources.
    /// </summary>
    public class Organization : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name of the organization.
        /// Must be unique across the system.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the organization.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Navigation property: collection of users associated with this organization.
        /// </summary>
        public ICollection<OrgUser> OrgUsers { get; set; } = new List<OrgUser>();

        /// <summary>
        /// Initializes a new instance of the Organization class.
        /// </summary>
        public Organization() : base()
        {
        }

        /// <summary>
        /// Creates a new organization with the specified name.
        /// </summary>
        /// <param name="name">The name of the organization.</param>
        public Organization(string name) : base()
        {
            Name = name;
        }
    }
}
