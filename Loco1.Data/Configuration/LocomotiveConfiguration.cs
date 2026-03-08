using Loco1.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loco1.Data.Configuration
    {
    public class LocomotiveConfiguration : IEntityTypeConfiguration<Locomotive>
        {
        public void Configure(EntityTypeBuilder<Locomotive> builder)
            {
            builder.Property(x => x.Number)
                   .IsRequired()
                   .HasMaxLength(6);

            builder.HasIndex(x => x.Number)
                   .IsUnique()
                   .HasDatabaseName("UX_Locomotive_Number");

             

            builder.HasMany(l => l.ShiftWorks)
                   .WithOne(sw=> sw.Locomotive)
                   .HasForeignKey(sw => sw.LocomotiveId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(l => l.Fuels)
                   .WithOne(f => f.Locomotive)
                   .HasForeignKey(f => f.LocomotiveId)
                   .OnDelete(DeleteBehavior.Restrict);
            }
        }
    }