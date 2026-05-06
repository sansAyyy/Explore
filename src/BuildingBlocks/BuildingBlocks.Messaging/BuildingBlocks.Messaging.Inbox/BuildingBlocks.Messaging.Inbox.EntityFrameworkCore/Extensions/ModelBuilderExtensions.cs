using BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder AddInboxMessageEntity(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        return modelBuilder;
    }
}

