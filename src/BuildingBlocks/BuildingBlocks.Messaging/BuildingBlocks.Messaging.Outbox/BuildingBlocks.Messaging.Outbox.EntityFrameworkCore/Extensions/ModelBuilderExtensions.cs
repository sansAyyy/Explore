using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Configurations;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder AddOutboxMessageEntity(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        return modelBuilder;
    }
}

