using Explore.MessageCenterService.Domain.MessageTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.MessageCenterService.Infrastructure.Persistence.Configurations;

public sealed class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
{
    public void Configure(EntityTypeBuilder<MessageTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(256);
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.ChannelType).IsRequired();
        builder.Property(x => x.TitleTemplate).HasMaxLength(256);
        builder.Property(x => x.BodyTemplate).HasMaxLength(4000).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

