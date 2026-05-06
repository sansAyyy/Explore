using System.Text.Json;
using BuildingBlocks.Messaging.Abstractions.Abstractions;
using BuildingBlocks.Messaging.Abstractions.Envelope;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Models;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Dispatchers;

public sealed class EntityFrameworkOutboxDispatcher<TDbContext>
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly IEnvelopePublisher _envelopePublisher;
    private readonly OutboxDispatcherOptions _options;
    private readonly ILogger<EntityFrameworkOutboxDispatcher<TDbContext>> _logger;

    public EntityFrameworkOutboxDispatcher(
        TDbContext dbContext,
        IEnvelopePublisher envelopePublisher,
        IOptions<OutboxDispatcherOptions> options,
        ILogger<EntityFrameworkOutboxDispatcher<TDbContext>> logger)
    {
        _dbContext = dbContext;
        _envelopePublisher = envelopePublisher;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> DispatchBatchAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var lockUntil = now.Add(_options.LockTimeout);

        var candidateMessageIds = await _dbContext.Set<OutboxMessage>()
            .Where(x => x.ProcessedAt == null && (x.LockedUntil == null || x.LockedUntil < now))
            .OrderBy(x => x.OccurredOn)
            .Select(x => x.MessageId)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        var dispatchedCount = 0;

        foreach (var messageId in candidateMessageIds)
        {
            if (!await TryClaimAsync(messageId, now, lockUntil, cancellationToken))
            {
                continue;
            }

            var message = await _dbContext.Set<OutboxMessage>()
                .SingleAsync(x => x.MessageId == messageId, cancellationToken);

            try
            {
                await _envelopePublisher.PublishAsync(ToEnvelope(message), cancellationToken);
                message.ProcessedAt = DateTime.UtcNow;
                message.LockedUntil = null;
                message.LastError = null;
                await _dbContext.SaveChangesAsync(cancellationToken);
                dispatchedCount++;
            }
            catch (Exception exception)
            {
                message.AttemptCount++;
                message.LastError = Truncate(exception.Message, 4000);
                message.LockedUntil = null;
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    exception,
                    "Failed to dispatch outbox message. DbContext: {DbContextName}, MessageId: {MessageId}, PayloadType: {PayloadType}",
                    typeof(TDbContext).Name,
                    message.MessageId,
                    message.PayloadType);
            }
        }

        return dispatchedCount;
    }

    private async Task<bool> TryClaimAsync(
        Guid messageId,
        DateTime now,
        DateTime lockUntil,
        CancellationToken cancellationToken)
    {
        var affectedRows = await _dbContext.Set<OutboxMessage>()
            .Where(x =>
                x.MessageId == messageId &&
                x.ProcessedAt == null &&
                (x.LockedUntil == null || x.LockedUntil < now))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.LockedUntil, lockUntil),
                cancellationToken);

        return affectedRows == 1;
    }

    private static object ToEnvelope(OutboxMessage message)
    {
        var payloadType = Type.GetType(message.PayloadType, throwOnError: true)
            ?? throw new InvalidOperationException($"Payload type '{message.PayloadType}' could not be resolved.");
        var payload = JsonSerializer.Deserialize(message.PayloadJson, payloadType)
            ?? throw new InvalidOperationException($"Payload json for type '{message.PayloadType}' could not be deserialized.");

        var envelopeType = typeof(MessageEnvelope<>).MakeGenericType(payloadType);
        var envelope = Activator.CreateInstance(envelopeType)
            ?? throw new InvalidOperationException($"Envelope instance for type '{payloadType.FullName}' could not be created.");

        envelopeType.GetProperty(nameof(MessageEnvelope<object>.MessageId))!.SetValue(envelope, message.MessageId);
        envelopeType.GetProperty(nameof(MessageEnvelope<object>.OccurredOn))!.SetValue(envelope, message.OccurredOn);
        envelopeType.GetProperty(nameof(MessageEnvelope<object>.CorrelationId))!.SetValue(envelope, message.CorrelationId);
        envelopeType.GetProperty(nameof(MessageEnvelope<object>.Payload))!.SetValue(envelope, payload);
        return envelope;
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}

