using System.Linq.Expressions;                           // for Expression in query filter
using System.Security.Claims;                            // for ClaimTypes.Name
using Loco1.Data.Models;
using Microsoft.AspNetCore.Http;                         // for IHttpContextAccessor
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Loco1.Data
{
    // Note: We type the IdentityDbContext with ApplicationUser
    public class LocoDbContext(
        DbContextOptions<LocoDbContext> options,
        IHttpContextAccessor http) : IdentityDbContext<ApplicationUser>(options)
    {
        private readonly IHttpContextAccessor _http = http;     // for current user

        // Domain sets
        public DbSet<Locomotive> Locomotives { get; set; } = null!;
        public DbSet<Fuel> Fuels { get; set; } = null!;
        public DbSet<ShiftWork> ShiftWorks { get; set; } = null!;

        // Audit
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Keep Identity defaults

            // Apply IEntityTypeConfiguration<> from this assembly
            builder.ApplyConfigurationsFromAssembly(typeof(LocoDbContext).Assembly);

            // ------------------------------------------------------------
            // Global query filter: exclude soft-deleted rows for all entities
            // inheriting from AuditEntity (e => !e.IsDeleted)
            // ------------------------------------------------------------
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(AuditEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var isDeleted = Expression.Property(parameter, nameof(AuditEntity.IsDeleted));
                    var condition = Expression.Equal(isDeleted, Expression.Constant(false));
                    var lambda = Expression.Lambda(condition, parameter);

                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAudit();                                // set Created*/Modified* and soft-delete
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            ApplyAudit();                                // set Created*/Modified* and soft-delete
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        // ------------------------------------------------------------
        // Audit & soft-delete:
        //  - Added    -> set CreatedOn/CreatedBy, force IsDeleted=false
        //  - Modified -> keep Created*, set ModifiedOn/ModifiedBy
        //  - Deleted  -> convert to soft delete (state=Modified, IsDeleted=true)
        // ------------------------------------------------------------
        private void ApplyAudit()
        {
            var now = DateTime.UtcNow;   // always UTC
            var user =
                _http.HttpContext?.User?.Identity?.Name
                ?? _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
                ?? "system";

            foreach (var entry in ChangeTracker.Entries<AuditEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = now;
                        entry.Entity.CreatedBy = user;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        // Never overwrite Created*
                        entry.Property(x => x.CreatedOn).IsModified = false;
                        entry.Property(x => x.CreatedBy).IsModified = false;

                        entry.Entity.ModifiedOn = now;
                        entry.Entity.ModifiedBy = user;

                        // If restoring from soft-delete
                        if (!entry.Entity.IsDeleted)
                        {
                            entry.Entity.DateDeleted = null;  // <-- NEW
                            entry.Entity.DeletedBy = null;    // <-- NEW
                        }
                        break;

                    case EntityState.Deleted:
                        // Soft delete instead of physical delete
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.ModifiedOn = now;
                        entry.Entity.ModifiedBy = user;

                        entry.Entity.DateDeleted = now;       // <-- NEW
                        entry.Entity.DeletedBy = user;        // <-- NEW
                        break;
                }
            }
        }
    }
        }
 