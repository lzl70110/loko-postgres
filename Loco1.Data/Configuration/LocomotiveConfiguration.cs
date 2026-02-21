using Loco1.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loco1.Data.Configuration
    {
    // Fluent config for Locomotive (unique number + safe delete behavior)
    public class LocomotiveConfiguration : IEntityTypeConfiguration<Locomotive>
        {
        public void Configure(EntityTypeBuilder<Locomotive> builder)
            {
            // Number must be unique
            builder.HasIndex(x => x.Number)
                   .IsUnique()
                   .HasDatabaseName("UX_Locomotive_Number");

            // 1..many ShiftWork (restrict delete)
            builder.HasMany(l => l.ShiftWorks)
                   .WithOne(sw => sw.Locomotive)
                   .HasForeignKey(sw => sw.LocomotiveId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1..many Fuel (restrict delete)
            builder.HasMany(l => l.Fuels)
                   .WithOne(f => f.Locomotive)
                   .HasForeignKey(f => f.LocomotiveId)
                   .OnDelete(DeleteBehavior.Restrict);
            }
        }
    }