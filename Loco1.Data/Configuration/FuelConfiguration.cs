using Loco1.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loco1.Data.Configuration
    {
    // Fluent config for Fuel (fast lookups per loco + date)
    public class FuelConfiguration : IEntityTypeConfiguration<Fuel>
        {
        public void Configure(EntityTypeBuilder<Fuel> builder)
            {
            // Common reporting/search pattern
            builder.HasIndex(x => new { x.LocomotiveId, x.RecordedOn })
                   .HasDatabaseName("IX_Fuel_Loco_Date");
            }
        }
    }