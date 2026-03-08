using Loco1.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loco1.Data.Configuration
    {
    // EN: Fluent configuration for AuditLog entity
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
        {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
            {
            // EN: Explicit table mapping (optional, but recommended for clarity)
            // builder.ToTable("AuditLogs");

            // EN: Set fractional seconds precision; PostgreSQL -> timestamp(3) without tz
            builder.Property(a => a.Timestamp)
                   .HasPrecision(3)
                   .HasDefaultValueSql("NOW()"); // EN: server-side default

            // EN: Optional - prevent mapping problems if 'User' becomes reserved keyword
            // builder.Property(a => a.User).HasColumnName("UserName");

            // EN: Keys are assumed from model (Id). If you want explicit:
            // builder.HasKey(a => a.Id);

            // EN: No FK config needed – conventions discover navigation props automatically
            }
        }
    }