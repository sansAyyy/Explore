using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Common.Http;
using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using Explore.AdminIdentityService.Application.Abstractions.Notifications;
using Explore.AdminIdentityService.Infrastructure.MessageCenter;
using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;

namespace Explore.AdminIdentityService.Application.Tests.Infrastructure;

public sealed class MessageCenterClientTests
{
    [Fact]
    public async Task SendPhoneLoginCodeAsync_ShouldBuildExpectedRequest()
    {
        var handler = new CapturingHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty)
            });

        var client = CreateClient(handler, TimeSpan.FromSeconds(1), "corr-001");

        var result = await client.SendPhoneLoginCodeAsync(
            "13800138000",
            "666666",
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Equal("http://localhost:5204/api/notifications/send", handler.LastRequest.RequestUri!.ToString());
        Assert.True(handler.LastRequest.Headers.TryGetValues("X-Correlation-ID", out var correlationValues));
        Assert.Equal("corr-001", Assert.Single(correlationValues));

        var payload = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal(AdminIdentitySmsTemplateCodes.PhoneLoginCode, payload.RootElement.GetProperty("templateCode").GetString());
        Assert.Equal(1, payload.RootElement.GetProperty("channel").GetInt32());
        Assert.Equal("13800138000", payload.RootElement.GetProperty("recipient").GetProperty("phoneNumber").GetString());
        Assert.Equal("666666", payload.RootElement.GetProperty("parameters").GetProperty("code").GetString());
        Assert.Equal("5", payload.RootElement.GetProperty("parameters").GetProperty("expireMinutes").GetString());
    }

    [Fact]
    public async Task SendPhoneLoginCodeAsync_ShouldReturnFailure_WhenMessageCenterReturnsProblemDetails()
    {
        var handler = new CapturingHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(
                    """{"title":"message center unavailable","status":400}""",
                    Encoding.UTF8,
                    "application/problem+json")
            });
        var client = CreateClient(handler, TimeSpan.FromSeconds(1));

        var result = await client.SendPhoneLoginCodeAsync(
            "13800138000",
            "666666",
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.BadRequest, result.Error.Code);
        Assert.Equal("message center unavailable", result.Error.Message);
    }

    [Fact]
    public async Task SendPhoneLoginCodeAsync_ShouldNotRetry_WhenDownstreamFails()
    {
        var handler = new CapturingHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(string.Empty)
            });
        var client = CreateClient(handler, TimeSpan.FromSeconds(1));

        var result = await client.SendPhoneLoginCodeAsync(
            "13800138000",
            "666666",
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task SendPhoneLoginCodeAsync_ShouldReturnStableTimeoutMessage_WhenRequestTimesOut()
    {
        var handler = new CapturingHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty)
            };
        });
        var client = CreateClient(handler, TimeSpan.FromMilliseconds(50));

        var result = await client.SendPhoneLoginCodeAsync(
            "13800138000",
            "666666",
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Message center request timed out.", result.Error.Message);
    }

    [Fact]
    public void CreateError_ShouldReturnStableMessage_WhenCircuitIsOpen()
    {
        var error = OutboundHttpFailureMapper.CreateError(new BrokenCircuitException(), "Message center");

        Assert.Equal(ErrorCodes.BadRequest, error.Code);
        Assert.Equal("Message center is temporarily unavailable.", error.Message);
    }

    private static IAdminMessageCenterClient CreateClient(
        CapturingHttpMessageHandler handler,
        TimeSpan timeout,
        string? correlationId = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICorrelationContextAccessor>(new TestCorrelationContextAccessor
        {
            CorrelationId = correlationId
        });

        services.AddTypedHttpClient<IAdminMessageCenterClient, MessageCenterClient>(
                new Uri("http://localhost:5204", UriKind.Absolute),
                timeout,
                OutboundHttpResilienceProfile.CommandNoRetry)
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IAdminMessageCenterClient>();
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public CapturingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            : this((request, _) => Task.FromResult(handler(request)))
        {
        }

        public CapturingHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        public int CallCount { get; private set; }

        public HttpRequestMessage? LastRequest { get; private set; }

        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequest = request;
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return await _handler(request, cancellationToken);
        }
    }

    private sealed class TestCorrelationContextAccessor : ICorrelationContextAccessor
    {
        public string? CorrelationId { get; set; }
    }
}

