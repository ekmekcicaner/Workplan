using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workplan.Domain.Entities;

namespace Workplan.Infrastructure.Persistence.Configurations;

public class CrewMemberConfiguration : IEntityTypeConfiguration<CrewMember>
{
    public void Configure(EntityTypeBuilder<CrewMember> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PersonnelRef)
            .HasMaxLength(100)
            .IsRequired();
    }
}
