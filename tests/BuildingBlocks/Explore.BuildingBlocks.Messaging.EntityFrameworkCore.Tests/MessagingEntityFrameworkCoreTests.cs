using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using BuildingBlocks.Messaging.Abstractions.Abstractions;
using BuildingBlocks.Messaging.Abstractions.Envelope;
using BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Extensions;
using BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Models;
using BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Processors;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Dispatchers;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Extensions;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Models;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Options;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Writers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class MessagingEntityFrameworkCoreTests
{
    [Fact]
    public async Task OutboxWriter_ShouldPersistExpectedEnvelopeMetadata()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var dbContext = CreateDbContext(connection);
        await dbContext.Database.EnsureCreatedAsync();

        var writer = new EntityFrameworkOutboxMessageWriter<TestMessagingDbContext>(
            dbContext,
            new TestCorrelationContextAccessor { CorrelationId = "corr-001" });

        await writer.WriteAsync(new TestMessage("created"), CancellationToken.None);
        await dbContext.SaveChangesAsync();

        var outboxMessage = await dbContext.OutboxMessages.SingleAsync();
        Assert.NotEqual(Guid.Empty, outboxMessage.MessageId);
        Assert.Equal(typeof(TestMessage).AssemblyQualifiedName, outboxMessage.PayloadType);
        Assert.Contains("created", outboxMessage.PayloadJson);
        Assert.Equal("corr-001", outboxMessage.CorrelationId);
        Assert.Equal(0, outboxMessage.AttemptCount);
        Assert.Null(outboxMessage.ProcessedAt);
    }

    [Fact]
    public async Task OutboxDispatcher_ShouldKeepMessageIdStableAcrossRetry()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var storedMessageId = Guid.Empty;

        await using (var dbContext = CreateDbContext(connection))
        {
            await dbContext.Database.EnsureCreatedAsync();

            var writer = new EntityFrameworkOutboxMessageWriter<TestMessagingDbContext>(
                dbContext,
                new TestCorrelationContextAccessor());
            await writer.WriteAsync(new TestMessage("retry"), CancellationToken.None);
            await dbContext.SaveChangesAsync();

            storedMessageId = await dbContext.OutboxMessages.Select(x => x.MessageId).SingleAsync();
        }

        var publisher = new CapturingEnvelopePublisher(failuresBeforeSuccess: 1);

        await using (var firstDispatchDbContext = CreateDbContext(connection))
        {
            var dispatcher = CreateDispatcher(firstDispatchDbContext, publisher);
            await dispatcher.DispatchBatchAsync();
        }

        await using (var secondDispatchDbContext = CreateDbContext(connection))
        {
            var dispatcher = CreateDispatcher(secondDispatchDbContext, publisher);
            await dispatcher.DispatchBatchAsync();
        }

        Assert.Equal(1, publisher.PublishedCount);
        Assert.Equal(storedMessageId, publisher.PublishedEnvelopeMessageId);

        await using var assertionDbContext = CreateDbContext(connection);
        var outboxMessage = await assertionDbContext.OutboxMessages.SingleAsync();
        Assert.Equal(1, outboxMessage.AttemptCount);
        Assert.NotNull(outboxMessage.ProcessedAt);
        Assert.Null(outboxMessage.LockedUntil);
    }

    [Fact]
    public async Task InboxProcessor_ShouldSkipDuplicateMessage()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var dbContext = CreateDbContext(connection);
        await dbContext.Database.EnsureCreatedAsync();

        var processor = new EntityFrameworkInboxMessageProcessor<TestMessagingDbContext>(dbContext);
        var envelope = MessageEnvelope<TestMessage>.Create(
            new TestMessage("duplicate"),
            Guid.NewGuid(),
            DateTime.UtcNow,
            "corr-002");
        var handledCount = 0;

        var first = await processor.ProcessAsync(
            envelope,
            "TestConsumer",
            _ =>
            {
                handledCount++;
                return Task.CompletedTask;
            });

        var second = await processor.ProcessAsync(
            envelope,
            "TestConsumer",
            _ =>
            {
                handledCount++;
                return Task.CompletedTask;
            });

        Assert.True(first);
        Assert.False(second);
        Assert.Equal(1, handledCount);
        Assert.Single(dbContext.InboxMessages);
    }

    [Fact]
    public async Task OutboxDispatcher_ShouldNotPublishDuplicateMessage_WhenTwoDispatchersRunConcurrently()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"outbox-tests-{Guid.NewGuid():N}.db");
        try
        {
            var connectionString = $"Data Source={databasePath}";

            await using (var setupConnection = new SqliteConnection(connectionString))
            {
                await setupConnection.OpenAsync();
                await using var setupContext = CreateDbContext(setupConnection);
                await setupContext.Database.EnsureCreatedAsync();
                var writer = new EntityFrameworkOutboxMessageWriter<TestMessagingDbContext>(
                    setupContext,
                    new TestCorrelationContextAccessor());
                await writer.WriteAsync(new TestMessage("parallel"), CancellationToken.None);
                await setupContext.SaveChangesAsync();
            }

            var publisher = new CapturingEnvelopePublisher();

            await using var connection1 = new SqliteConnection(connectionString);
            await using var connection2 = new SqliteConnection(connectionString);
            await connection1.OpenAsync();
            await connection2.OpenAsync();

            await using var dbContext1 = CreateDbContext(connection1);
            await using var dbContext2 = CreateDbContext(connection2);

            var dispatcher1 = CreateDispatcher(dbContext1, publisher);
            var dispatcher2 = CreateDispatcher(dbContext2, publisher);

            await Task.WhenAll(
                dispatcher1.DispatchBatchAsync(),
                dispatcher2.DispatchBatchAsync());

            Assert.Equal(1, publisher.PublishedCount);
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                try
                {
                    File.Delete(databasePath);
                }
                catch (IOException)
                {
                }
            }
        }
    }

    private static TestMessagingDbContext CreateDbContext(SqliteConnection connection)
    {
        return new TestMessagingDbContext(
            new DbContextOptionsBuilder<TestMessagingDbContext>()
                .UseSqlite(connection)
                .Options);
    }

    private static EntityFrameworkOutboxDispatcher<TestMessagingDbContext> CreateDispatcher(
        TestMessagingDbContext dbContext,
        CapturingEnvelopePublisher publisher)
    {
        return new EntityFrameworkOutboxDispatcher<TestMessagingDbContext>(
            dbContext,
            publisher,
            Options.Create(new OutboxDispatcherOptions
            {
                BatchSize = 50,
                PollInterval = TimeSpan.FromMilliseconds(10),
                LockTimeout = TimeSpan.FromSeconds(30)
            }),
            NullLogger<EntityFrameworkOutboxDispatcher<TestMessagingDbContext>>.Instance);
    }

    private sealed record TestMessage(string Value);

    private sealed class TestMessagingDbContext : DbContext
    {
        public TestMessagingDbContext(DbContextOptions<TestMessagingDbContext> options)
            : base(options)
        {
        }

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddInboxMessageEntity();
        }
    }

    private sealed class TestCorrelationContextAccessor : ICorrelationContextAccessor
    {
        public string? CorrelationId { get; set; }
    }

    private sealed class CapturingEnvelopePublisher : IEnvelopePublisher
    {
        private readonly int _failuresBeforeSuccess;
        private int _attemptCount;

        public CapturingEnvelopePublisher(int failuresBeforeSuccess = 0)
        {
            _failuresBeforeSuccess = failuresBeforeSuccess;
        }

        public int PublishedCount { get; private set; }

        public Guid PublishedEnvelopeMessageId { get; private set; }

        public Task PublishAsync(object envelope, CancellationToken cancellationToken = default)
        {
            _attemptCount++;
            if (_attemptCount <= _failuresBeforeSuccess)
            {
                throw new InvalidOperationException("Simulated transport failure.");
            }

            PublishedCount++;
            PublishedEnvelopeMessageId = (Guid)envelope.GetType().GetProperty(nameof(MessageEnvelope<TestMessage>.MessageId))!.GetValue(envelope)!;
            return Task.CompletedTask;
        }
    }
}



