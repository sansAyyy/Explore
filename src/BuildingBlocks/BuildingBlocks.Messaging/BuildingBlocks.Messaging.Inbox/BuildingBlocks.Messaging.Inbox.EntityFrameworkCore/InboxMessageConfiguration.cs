using BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Configurations;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(x => new { x.MessageId, x.ConsumerName });

        builder.Property(x => x.ConsumerName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PayloadType).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(128);
        builder.Property(x => x.ProcessedAt).IsRequired();
    }
}

