using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workplan.Domain.Entities;

namespace Workplan.Infrastructure.Persistence.Configurations;

public class CrewRegionConfiguration : IEntityTypeConfiguration<CrewRegion>
{
    public void Configure(EntityTypeBuilder<CrewRegion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(x => x.Locations)
            .WithOne(x => x.CrewRegion)
            .HasForeignKey(x => x.CrewRegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
