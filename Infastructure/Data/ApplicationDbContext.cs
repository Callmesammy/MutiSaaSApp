using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infastructure.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for the multi-tenant application.
    /// Configures entity mappings, relationships, and soft delete filters.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationDbContext class.
        /// </summary>
        /// <param name="options">The DbContext options.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Organizations DbSet.
        /// </summary>
        public DbSet<Organization> Organizations { get; set; }

        /// <summary>
        /// Gets or sets the Users DbSet.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the OrgUsers DbSet.
        /// </summary>
        public DbSet<OrgUser> OrgUsers { get; set; }

        /// <summary>
        /// Gets or sets the InviteTokens DbSet.
        /// </summary>
        public DbSet<InviteToken> InviteTokens { get; set; }

        /// <summary>
        /// Gets or sets the TaskItems DbSet.
        /// </summary>
        public DbSet<TaskItem> TaskItems { get; set; }

        /// <summary>
        /// Configures the model for the database context.
        /// Sets up entity configurations, relationships, indexes, and soft delete filters.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Organization configuration
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(256);
                entity.Property(e => e.LastName).HasMaxLength(256);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // OrgUser configuration
            modelBuilder.Entity<OrgUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrganizationId).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Role).IsRequired();

                // Foreign keys
                entity.HasOne(e => e.Organization)
                    .WithMany(o => o.OrgUsers)
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.OrgUsers)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint: one user per organization
                entity.HasIndex(e => new { e.OrganizationId, e.UserId }).IsUnique();

                // Indexes for filtering by user and role
                entity.HasIndex(e => new { e.UserId, e.Role });
                entity.HasIndex(e => e.UserId);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // InviteToken configuration
            modelBuilder.Entity<InviteToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.OrganizationId).IsRequired();
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.IsUsed).IsRequired().HasDefaultValue(false);

                // Unique index on token
                entity.HasIndex(e => e.Token).IsUnique();

                // Indexes for invitation lookup and filtering
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => new { e.OrganizationId, e.IsUsed, e.ExpiresAt });

                // Index for finding unexpired invites
                entity.HasIndex(e => new { e.Email, e.IsUsed });

                // Foreign key to Organization
                entity.HasOne(e => e.Organization)
                    .WithMany()
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key to InvitedByUser
                entity.HasOne(e => e.InvitedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.InvitedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Foreign key to AcceptedByUser
                entity.HasOne(e => e.AcceptedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.AcceptedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // TaskItem configuration
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Priority).IsRequired();
                entity.Property(e => e.OrganizationId).IsRequired();
                entity.Property(e => e.CreatedByUserId).IsRequired();
                entity.Property(e => e.DueDate).IsRequired(false);

                // Indexes for performance - query patterns
                entity.HasIndex(e => e.OrganizationId);
                entity.HasIndex(e => new { e.OrganizationId, e.Status });
                entity.HasIndex(e => new { e.OrganizationId, e.Priority });
                entity.HasIndex(e => new { e.OrganizationId, e.CreatedAt }).IsDescending(false, true);
                entity.HasIndex(e => e.AssigneeId);
                entity.HasIndex(e => e.CreatedByUserId);

                // Composite indexes for filtering
                entity.HasIndex(e => new { e.OrganizationId, e.Status, e.Priority });
                entity.HasIndex(e => new { e.OrganizationId, e.AssigneeId });

                // Index for pagination and sorting
                entity.HasIndex(e => new { e.OrganizationId, e.CreatedAt });

                // Foreign key to Organization
                entity.HasOne(e => e.Organization)
                    .WithMany()
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key to CreatedByUser
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Foreign key to Assignee (nullable)
                entity.HasOne(e => e.Assignee)
                    .WithMany()
                    .HasForeignKey(e => e.AssigneeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });
        }
    }
}

