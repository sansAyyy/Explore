using BuildingBlocks.Correlation.Abstractions.Constants;
using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace BuildingBlocks.Common.Http;

public static class OutboundHttpClientRegistrationExtensions
{
    public static IHttpClientBuilder AddTypedHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        Uri baseAddress,
        TimeSpan timeout,
        OutboundHttpResilienceProfile resilienceProfile,
        Action<HttpClient>? configureClient = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(baseAddress);

        if (!baseAddress.IsAbsoluteUri)
        {
            throw new InvalidOperationException("Outbound HTTP client BaseAddress must be an absolute URI.");
        }

        if (timeout <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Outbound HTTP client Timeout must be greater than zero.");
        }

        services.AddTransient<CorrelationIdPropagationHandler>();

        var builder = services.AddHttpClient<TClient, TImplementation>(client =>
        {
            client.BaseAddress = baseAddress;
            client.Timeout = Timeout.InfiniteTimeSpan;
            configureClient?.Invoke(client);
        });

        builder.AddHttpMessageHandler<CorrelationIdPropagationHandler>();
        builder.AddStandardResilienceHandler(options =>
        {
            ConfigureStandardResilience(options, timeout, resilienceProfile);
        });

        return builder;
    }

    private static void ConfigureStandardResilience(
        HttpStandardResilienceOptions options,
        TimeSpan timeout,
        OutboundHttpResilienceProfile profile)
    {
        var minimumSamplingDuration = timeout + timeout;

        options.TotalRequestTimeout.Timeout = timeout;
        options.AttemptTimeout.Timeout = timeout;
        options.CircuitBreaker.MinimumThroughput = 2;
        options.CircuitBreaker.SamplingDuration = minimumSamplingDuration > TimeSpan.FromSeconds(10)
            ? minimumSamplingDuration
            : TimeSpan.FromSeconds(10);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);

        if (profile == OutboundHttpResilienceProfile.ReadOnly)
        {
            options.Retry.MaxRetryAttempts = 2;
            options.Retry.Delay = TimeSpan.FromMilliseconds(200);
            options.Retry.UseJitter = true;
            return;
        }

        options.Retry.MaxRetryAttempts = 2;
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);
        options.Retry.UseJitter = true;
        options.Retry.DisableForUnsafeHttpMethods();
    }

    private sealed class CorrelationIdPropagationHandler : DelegatingHandler
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public CorrelationIdPropagationHandler(ICorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = _correlationContextAccessor.CorrelationId;
            if (!string.IsNullOrWhiteSpace(correlationId) &&
                !request.Headers.Contains(CorrelationIdConstants.HeaderName))
            {
                request.Headers.Add(CorrelationIdConstants.HeaderName, correlationId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}

