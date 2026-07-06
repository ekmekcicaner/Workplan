using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workplan.Domain.Entities;

namespace Workplan.Infrastructure.Persistence.Configurations;

public class WorkItemTypeConfiguration : IEntityTypeConfiguration<WorkItemType>
{
    public void Configure(EntityTypeBuilder<WorkItemType> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.Unit).HasConversion<string>().HasMaxLength(20);

        // Kendi kendine ilişki (Self-Referencing) Aynen klasör ağacı mantığı
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict); // Üst kırılım silinince altlar patlamasın diye koruma
    }
}