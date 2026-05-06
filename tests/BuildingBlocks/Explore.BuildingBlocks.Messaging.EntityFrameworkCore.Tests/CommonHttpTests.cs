using System.Net;
using BuildingBlocks.Common.Http;
using BuildingBlocks.Correlation.Abstractions.Constants;
using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class CommonHttpTests
{
    [Fact]
    public async Task AddTypedHttpClient_ShouldPropagateCorrelationHeader()
    {
        var services = new ServiceCollection();
        var handler = new CaptureHandler();

        services.AddSingleton<ICorrelationContextAccessor>(new FakeCorrelationContextAccessor
        {
            CorrelationId = "corr-123"
        });

        services.AddTypedHttpClient<ITestClient, TestClient>(
                new Uri("https://example.com"),
                TimeSpan.FromSeconds(1),
                OutboundHttpResilienceProfile.ReadOnly)
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<ITestClient>();

        await client.SendAsync(CancellationToken.None);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("corr-123", handler.LastRequest!.Headers.GetValues(CorrelationIdConstants.HeaderName).Single());
    }

    [Fact]
    public void AddTypedHttpClient_ShouldRejectRelativeBaseAddress()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICorrelationContextAccessor>(new FakeCorrelationContextAccessor());

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddTypedHttpClient<ITestClient, TestClient>(
                new Uri("/relative", UriKind.Relative),
                TimeSpan.FromSeconds(1),
                OutboundHttpResilienceProfile.ReadOnly));

        Assert.Equal("Outbound HTTP client BaseAddress must be an absolute URI.", exception.Message);
    }

    [Fact]
    public void AddTypedHttpClient_ShouldRejectNonPositiveTimeout()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICorrelationContextAccessor>(new FakeCorrelationContextAccessor());

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddTypedHttpClient<ITestClient, TestClient>(
                new Uri("https://example.com"),
                TimeSpan.Zero,
                OutboundHttpResilienceProfile.ReadOnly));

        Assert.Equal("Outbound HTTP client Timeout must be greater than zero.", exception.Message);
    }

    [Fact]
    public void ToRequiredAbsoluteUri_ShouldRejectNonAbsoluteUri()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            "relative/path".ToRequiredAbsoluteUri("Services:MessageCenter"));

        Assert.Equal("Services:MessageCenter must be an absolute URI.", exception.Message);
    }

    private interface ITestClient
    {
        Task SendAsync(CancellationToken cancellationToken);
    }

    private sealed class TestClient : ITestClient
    {
        private readonly HttpClient _httpClient;

        public TestClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task SendAsync(CancellationToken cancellationToken)
        {
            return _httpClient.GetAsync("/", cancellationToken);
        }
    }

    private sealed class FakeCorrelationContextAccessor : ICorrelationContextAccessor
    {
        public string? CorrelationId { get; set; } = string.Empty;
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request
            });
        }
    }
}

