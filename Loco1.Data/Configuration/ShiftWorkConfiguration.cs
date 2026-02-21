using Loco1.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loco1.Data.Configuration
    {
    // Fluent config for ShiftWork (fast lookups per loco + date)
    public class ShiftWorkConfiguration : IEntityTypeConfiguration<ShiftWork>
        {
        public void Configure(EntityTypeBuilder<ShiftWork> builder)
            {
            // Common reporting/search pattern
            builder.HasIndex(x => new { x.LocomotiveId, x.ShiftDate })
                   .HasDatabaseName("IX_ShiftWork_Loco_Date");
            }
        }
    }