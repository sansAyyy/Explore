using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.MessageId);

        builder.Property(x => x.PayloadType).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.PayloadJson).IsRequired();
        builder.Property(x => x.OccurredOn).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(128);
        builder.Property(x => x.ProcessedAt);
        builder.Property(x => x.AttemptCount).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(4000);
        builder.Property(x => x.LockedUntil);

        builder.HasIndex(x => new { x.ProcessedAt, x.LockedUntil, x.OccurredOn });
    }
}

