using Explore.MessageCenterService.Domain.NotificationDispatches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.MessageCenterService.Infrastructure.Persistence.Configurations;

public sealed class NotificationDispatchConfiguration : IEntityTypeConfiguration<NotificationDispatch>
{
    public void Configure(EntityTypeBuilder<NotificationDispatch> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TemplateCode).HasMaxLength(128).IsRequired();
        builder.Property(x => x.RecipientAddressSnapshot).HasMaxLength(256);
        builder.Property(x => x.Title).HasMaxLength(256);
        builder.Property(x => x.Body).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.FailureReason).HasMaxLength(256);
        builder.Property(x => x.BusinessIdempotencyKey).HasMaxLength(128);
        builder.Property(x => x.RequestedAt).IsRequired();
        builder.Property(x => x.SentAt);
        builder.Property(x => x.Status).IsRequired();

        builder.HasIndex(x => new { x.RecipientUserId, x.RequestedAt });
    }
}

