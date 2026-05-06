using BuildingBlocks.Messaging.Abstractions.Envelope;
using BuildingBlocks.Messaging.Inbox.Abstractions;
using BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Processors;

public sealed class EntityFrameworkInboxMessageProcessor<TDbContext> : IInboxMessageProcessor
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    public EntityFrameworkInboxMessageProcessor(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ProcessAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        string consumerName,
        Func<CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(handler);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (!await TryRegisterInboxMessageAsync(envelope, consumerName, cancellationToken))
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await handler(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private async Task<bool> TryRegisterInboxMessageAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        string consumerName,
        CancellationToken cancellationToken)
        where TMessage : class
    {
        var trackedMessage = _dbContext.Set<InboxMessage>().Local
            .SingleOrDefault(x => x.MessageId == envelope.MessageId && x.ConsumerName == consumerName);
        if (trackedMessage is not null)
        {
            return false;
        }

        var inboxMessage = new InboxMessage
        {
            MessageId = envelope.MessageId,
            ConsumerName = consumerName,
            PayloadType = typeof(TMessage).AssemblyQualifiedName
                ?? throw new InvalidOperationException($"AssemblyQualifiedName is missing for type '{typeof(TMessage).FullName}'."),
            CorrelationId = envelope.CorrelationId,
            ProcessedAt = DateTime.UtcNow
        };

        await _dbContext.Set<InboxMessage>().AddAsync(inboxMessage, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            _dbContext.Entry(inboxMessage).State = EntityState.Detached;

            var exists = await _dbContext.Set<InboxMessage>().AnyAsync(
                x => x.MessageId == envelope.MessageId && x.ConsumerName == consumerName,
                cancellationToken);

            if (exists)
            {
                return false;
            }

            throw;
        }
    }
}

