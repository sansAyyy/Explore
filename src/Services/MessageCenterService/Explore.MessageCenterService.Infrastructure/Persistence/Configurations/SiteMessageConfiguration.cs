using Explore.MessageCenterService.Domain.SiteMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.MessageCenterService.Infrastructure.Persistence.Configurations;

public sealed class SiteMessageConfiguration : IEntityTypeConfiguration<SiteMessage>
{
    public void Configure(EntityTypeBuilder<SiteMessage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(256);
        builder.Property(x => x.Content).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.IsRead).IsRequired();
        builder.Property(x => x.ReadAt);

        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
    }
}

