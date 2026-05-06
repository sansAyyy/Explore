using System.Net;
using System.Net.Http;
using System.Text;
using BuildingBlocks.Common.Http;
using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using Explore.MessageCenterService.Application.Abstractions.External;
using Explore.MessageCenterService.Infrastructure.External;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.MessageCenterService.Application.Tests.Infrastructure.External;

public sealed class HttpRecipientDirectoryClientTests
{
    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnRecipientProfile()
    {
        var userId = Guid.NewGuid();
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $$"""{"userId":"{{userId}}","phoneNumber":"13800138000","miniProgramOpenId":null}""",
                    Encoding.UTF8,
                    "application/json")
            });
        var client = CreateClient(handler);

        var result = await client.GetByUserIdAsync(userId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value!.UserId);
        Assert.Equal("13800138000", result.Value.PhoneNumber);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal($"http://customer-account-service/api/users/{userId}/notification-profile", handler.LastRequest!.RequestUri!.ToString());
        Assert.True(handler.LastRequest.Headers.TryGetValues("X-Correlation-ID", out var correlationValues));
        Assert.Equal("corr-002", Assert.Single(correlationValues));
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldRetryTransientFailure()
    {
        var userId = Guid.NewGuid();
        var attempt = 0;
        var handler = new StubHttpMessageHandler(request =>
        {
            attempt++;

            return attempt == 1
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(string.Empty)
                }
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        $$"""{"userId":"{{userId}}","phoneNumber":"13800138000","miniProgramOpenId":null}""",
                        Encoding.UTF8,
                        "application/json")
                };
        });
        var client = CreateClient(handler);

        var result = await client.GetByUserIdAsync(userId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(handler.CallCount >= 2);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNotFound_WhenProfileDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(string.Empty)
            });
        var client = CreateClient(handler);

        var result = await client.GetByUserIdAsync(userId, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.NotFound, result.Error.Code);
        Assert.Equal($"Recipient profile '{userId}' was not found.", result.Error.Message);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnStableTimeoutMessage_WhenRequestTimesOut()
    {
        var userId = Guid.NewGuid();
        var handler = new StubHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $$"""{"userId":"{{userId}}","phoneNumber":"13800138000","miniProgramOpenId":null}""",
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var client = CreateClient(handler, TimeSpan.FromMilliseconds(50));

        var result = await client.GetByUserIdAsync(userId, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.BadRequest, result.Error.Code);
        Assert.Equal("Recipient directory request timed out.", result.Error.Message);
    }

    private static HttpRecipientDirectoryClient CreateClient(
        StubHttpMessageHandler handler,
        TimeSpan? timeout = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICorrelationContextAccessor>(new TestCorrelationContextAccessor
        {
            CorrelationId = "corr-002"
        });
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new RecipientDirectoryOptions
        {
            BaseUrl = "http://customer-account-service",
            ProfilePathTemplate = "/api/users/{userId}/notification-profile",
            Timeout = timeout ?? TimeSpan.FromSeconds(1)
        }));

        services.AddTypedHttpClient<HttpRecipientDirectoryClient, HttpRecipientDirectoryClient>(
                new Uri("http://customer-account-service", UriKind.Absolute),
                timeout ?? TimeSpan.FromSeconds(1),
                OutboundHttpResilienceProfile.ReadOnly)
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<HttpRecipientDirectoryClient>();
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            : this((request, _) => Task.FromResult(handler(request)))
        {
        }

        public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        public int CallCount { get; private set; }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            return await _handler(request, cancellationToken);
        }
    }

    private sealed class TestCorrelationContextAccessor : ICorrelationContextAccessor
    {
        public string? CorrelationId { get; set; }
    }
}

