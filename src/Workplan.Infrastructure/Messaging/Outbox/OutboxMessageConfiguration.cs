using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Workplan.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);

        builder.HasIndex(x => new
        {
            x.ProcessedOnUtc,
            x.PoisonedOnUtc,
            x.NextAttemptOnUtc
        });
    }
}
