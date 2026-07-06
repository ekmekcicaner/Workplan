using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workplan.Domain.Entities;

namespace Workplan.Infrastructure.Persistence.Configurations;

public class StatusTransitionConfiguration : IEntityTypeConfiguration<StatusTransition>
{
    public void Configure(EntityTypeBuilder<StatusTransition> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Note).HasMaxLength(2000);
    }
}
