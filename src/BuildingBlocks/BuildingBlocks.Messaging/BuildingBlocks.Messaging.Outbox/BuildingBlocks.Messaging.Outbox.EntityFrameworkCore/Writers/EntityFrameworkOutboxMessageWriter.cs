using System.Text.Json;
using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using BuildingBlocks.Messaging.Abstractions.Envelope;
using BuildingBlocks.Messaging.Outbox.Abstractions;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Writers;

public sealed class EntityFrameworkOutboxMessageWriter<TDbContext> : IOutboxMessageWriter
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public EntityFrameworkOutboxMessageWriter(
        TDbContext dbContext,
        ICorrelationContextAccessor correlationContextAccessor)
    {
        _dbContext = dbContext;
        _correlationContextAccessor = correlationContextAccessor;
    }

    public Task WriteAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);

        var envelope = MessageEnvelope<TMessage>.Create(
            message,
            Guid.NewGuid(),
            DateTime.UtcNow,
            _correlationContextAccessor.CorrelationId);

        return _dbContext.Set<OutboxMessage>().AddAsync(
            new OutboxMessage
            {
                MessageId = envelope.MessageId,
                PayloadType = typeof(TMessage).AssemblyQualifiedName
                    ?? throw new InvalidOperationException($"AssemblyQualifiedName is missing for type '{typeof(TMessage).FullName}'."),
                PayloadJson = JsonSerializer.Serialize(message),
                OccurredOn = envelope.OccurredOn,
                CorrelationId = envelope.CorrelationId,
                AttemptCount = 0
            },
            cancellationToken).AsTask();
    }
}

