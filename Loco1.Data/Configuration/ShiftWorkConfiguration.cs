using Loco1.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loco1.Data.Configuration
    {
    public class ShiftWorkConfiguration : IEntityTypeConfiguration<ShiftWork>
        {
        public void Configure(EntityTypeBuilder<ShiftWork> builder)
            {
            // Composite index for faster queries by locomotive/date
            builder.HasIndex(x => new { x.LocomotiveId, x.ShiftDate })
                   .HasDatabaseName("IX_ShiftWork_Loco_Date");
            }
        }
    }
