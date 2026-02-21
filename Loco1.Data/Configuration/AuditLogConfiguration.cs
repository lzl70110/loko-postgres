using Loco1.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loco1.Data.Configuration
    {
    // English: Fluent configuration for AuditLog entity
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
        {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
            {
            // Set fractional seconds precision; PostgreSQL -> timestamp(3) without time zone
            builder.Property(a => a.Timestamp)
                   .HasPrecision(3)
                   .HasDefaultValueSql("NOW()"); // server-side default current timestamp

            // Optional: avoid reserved word "User" as column name
            // builder.Property(a => a.User).HasColumnName("UserName");

            // Optional FKs if needed are discovered by conventions from navigation props
            }
        }
    }