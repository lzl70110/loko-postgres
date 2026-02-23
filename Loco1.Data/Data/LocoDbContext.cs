using Loco1.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Loco1.Data
    {
    // Note: We type the IdentityDbContext with IdentityUser (optional but clearer)
    
    public class LocoDbContext : IdentityDbContext<ApplicationUser>
        {
       
        public LocoDbContext(DbContextOptions<LocoDbContext> options)
            : base(options)
            {
            }

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
            }
        }
    }