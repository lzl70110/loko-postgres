using Loco1.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loco1.Data.Configuration
    {
    // EN: Fluent config for Fuel (optimized for date + locomotive queries)
    public class FuelConfiguration : IEntityTypeConfiguration<Fuel>
        {
        public void Configure(EntityTypeBuilder<Fuel> builder)
            {
            // EN: Composite index for fast daily/chronological queries per locomotive
            builder.HasIndex(x => new { x.LocomotiveId, x.RecordedOn })
                   .HasDatabaseName("IX_Fuel_Loco_Date");
             builder.HasIndex(x => new { x.LocomotiveId, x.RecordedOn })
                   .IsDescending(false, true); // Loco ascending, date descending
            }
        }
    }