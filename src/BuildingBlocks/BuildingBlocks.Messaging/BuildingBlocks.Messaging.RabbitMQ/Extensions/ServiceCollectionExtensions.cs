using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using BuildingBlocks.Messaging.Abstractions.Abstractions;
using BuildingBlocks.Messaging.RabbitMQ.ContextAccessors;
using BuildingBlocks.Messaging.RabbitMQ.Filters;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using BuildingBlocks.Messaging.RabbitMQ.Publishers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace BuildingBlocks.Messaging.RabbitMQ.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQ(
            this IServiceCollection services,
            IConfiguration configuration,
            Assembly consumerAssembly)
        {
            var rabbitMqOptions = configuration
                .GetSection("RabbitMqOptions")
                .Get<RabbitMqOptions>() ?? throw new InvalidOperationException("RabbitMqOptions configuration is missing.");

            services.AddMassTransit(option =>
            {
                // Discover consumers automatically.
                option.AddConsumers(consumerAssembly);
                option.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.VirtualHost, h =>
                    {
                        h.Username(rabbitMqOptions.Username);
                        h.Password(rabbitMqOptions.Password);
                    });

                    // Retry transient failures before surfacing the error.
                    cfg.UseMessageRetry(r => r
                        .Interval(3, TimeSpan.FromSeconds(5)));

                    // Limit consumer concurrency at the bus level.
                    cfg.ConcurrentMessageLimit = rabbitMqOptions.ConcurrentMessageLimit;

                    cfg.UseConsumeFilter(typeof(CorrelationConsumeFilter<>), context);
                    cfg.UsePublishFilter(typeof(CorrelationPublishFilter<>), context);

                    // Create exchanges and queues from discovered endpoints.
                    cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(true));

                    // Delay redelivery for messages that still fail after retry.
                    cfg.UseDelayedRedelivery(r => r.Interval(2, TimeSpan.FromSeconds(10)));
                });
            });

            services.TryAddScoped<ICorrelationContextAccessor, NullCorrelationContextAccessor>();
            services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
            services.AddScoped<IEnvelopePublisher, MassTransitEnvelopePublisher>();
            return services;
        }
    }
}

