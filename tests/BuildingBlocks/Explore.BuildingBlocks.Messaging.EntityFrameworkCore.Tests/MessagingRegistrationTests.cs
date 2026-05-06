using BuildingBlocks.Messaging.Abstractions.Abstractions;
using BuildingBlocks.Messaging.Inbox.Abstractions;
using BuildingBlocks.Messaging.Inbox.EntityFrameworkCore.Extensions;
using BuildingBlocks.Messaging.Outbox.Abstractions;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Dispatchers;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Extensions;
using BuildingBlocks.Messaging.RabbitMQ.Extensions;
using BuildingBlocks.Messaging.RabbitMQ.Publishers;
using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class MessagingRegistrationTests
{
    [Fact]
    public void AddEntityFrameworkOutbox_ShouldRegisterOutboxServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TestMessagingRegistrationDbContext>(options => options.UseSqlite(CreateConnection()));
        services.AddSingleton<ICorrelationContextAccessor>(new FakeCorrelationContextAccessor());
        services.AddSingleton<IEnvelopePublisher, FakeEnvelopePublisher>();

        services.AddEntityFrameworkOutbox<TestMessagingRegistrationDbContext>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IOutboxMessageWriter>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<EntityFrameworkOutboxDispatcher<TestMessagingRegistrationDbContext>>());
        Assert.Contains(
            provider.GetServices<IHostedService>(),
            service => service.GetType() == typeof(OutboxDispatcherHostedService<TestMessagingRegistrationDbContext>));
    }

    [Fact]
    public void AddEntityFrameworkInbox_ShouldRegisterInboxProcessor()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestMessagingRegistrationDbContext>(options => options.UseSqlite(CreateConnection()));

        services.AddEntityFrameworkInbox<TestMessagingRegistrationDbContext>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IInboxMessageProcessor>());
    }

    [Fact]
    public async Task AddRabbitMQ_ShouldRegisterMessagingPublishers()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RabbitMqOptions:HostName"] = "localhost",
                ["RabbitMqOptions:VirtualHost"] = "/",
                ["RabbitMqOptions:Username"] = "guest",
                ["RabbitMqOptions:Password"] = "guest",
                ["RabbitMqOptions:ConcurrentMessageLimit"] = "10"
            })
            .Build();

        services.AddRabbitMQ(configuration, typeof(MessagingRegistrationTests).Assembly);

        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsType<MassTransitEventPublisher>(scope.ServiceProvider.GetRequiredService<IEventPublisher>());
        Assert.IsType<MassTransitEnvelopePublisher>(scope.ServiceProvider.GetRequiredService<IEnvelopePublisher>());
    }

    private static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    private sealed class TestMessagingRegistrationDbContext : DbContext
    {
        public TestMessagingRegistrationDbContext(DbContextOptions<TestMessagingRegistrationDbContext> options)
            : base(options)
        {
        }
    }

    private sealed class FakeCorrelationContextAccessor : ICorrelationContextAccessor
    {
        public string? CorrelationId { get; set; }
    }

    private sealed class FakeEnvelopePublisher : IEnvelopePublisher
    {
        public Task PublishAsync(object envelope, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
