using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Workplan.Domain.Entities;

namespace Workplan.Infrastructure.Persistence.Configurations;

public class DailyPlanConfiguration : IEntityTypeConfiguration<DailyPlan>
{
    public void Configure(EntityTypeBuilder<DailyPlan> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlannedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.PlannedManDay).HasPrecision(18, 4);
        builder.Property(x => x.FactQuantity).HasPrecision(18, 4);
        builder.Property(x => x.FactManDay).HasPrecision(18, 4);
        builder.Property(x => x.Overtime).HasPrecision(18, 4);
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.Unit).HasConversion<string>().HasMaxLength(20);

        // Normal (owned değil) ilişki: yeni StatusTransition kayıtları zaten tracked olan bir
        // DailyPlan'a eklendiğinde EF'in bunu "Added" değil "Modified" sanmasını (client-generated
        // Guid key + disconnected graph) önlemek için handler'lar bunu ayrıca DbSet'e ekliyor.
        builder.HasMany(x => x.History)
            .WithOne()
            .HasForeignKey("DailyPlanId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.LocationId, x.WorkDate });
    }
}
