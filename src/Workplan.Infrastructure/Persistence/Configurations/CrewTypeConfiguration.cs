using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workplan.Domain.Entities;

namespace Workplan.Infrastructure.Persistence.Configurations;

public class CrewTypeConfiguration : IEntityTypeConfiguration<CrewType>
{
    public void Configure(EntityTypeBuilder<CrewType> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
