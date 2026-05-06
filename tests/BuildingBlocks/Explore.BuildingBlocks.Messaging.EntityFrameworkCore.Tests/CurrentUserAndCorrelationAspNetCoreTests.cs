using System.Security.Claims;
using BuildingBlocks.Correlation.Abstractions.Constants;
using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using BuildingBlocks.Correlation.AspNetCore.Extensions;
using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.CurrentUser.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class CurrentUserAndCorrelationAspNetCoreTests
{
    [Fact]
    public void AddAspNetCoreCurrentUser_ShouldRegisterCurrentUser()
    {
        var services = new ServiceCollection();

        services.AddAspNetCoreCurrentUser();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();

        Assert.NotNull(currentUser);
    }

    [Fact]
    public void CurrentUser_ShouldResolveUserIdAndAuditActor_FromNameIdentifierClaim()
    {
        var services = new ServiceCollection();
        services.AddAspNetCoreCurrentUser();

        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IHttpContextAccessor>();
        var userId = Guid.NewGuid();
        accessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            ], "test"))
        };

        using var scope = provider.CreateScope();
        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();

        Assert.Equal(userId, currentUser.UserId);
        Assert.Equal(userId.ToString(), currentUser.AuditActor);
    }

    [Fact]
    public void CurrentUser_ShouldReturnNullUserIdAndSystemAuditActor_WhenHttpContextIsMissing()
    {
        var services = new ServiceCollection();
        services.AddAspNetCoreCurrentUser();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();

        Assert.Null(currentUser.UserId);
        Assert.Equal("System", currentUser.AuditActor);
    }

    [Fact]
    public async Task AddCorrelationAndUseCorrelation_ShouldPopulateContextAndResponseHeader()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCorrelation();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var correlationAccessor = scope.ServiceProvider.GetRequiredService<ICorrelationContextAccessor>();
        var app = new ApplicationBuilder(scope.ServiceProvider);
        var responseFeature = new TestHttpResponseFeature();

        app.UseCorrelation();
        app.Run(context =>
        {
            Assert.True(context.Items.ContainsKey(CorrelationIdConstants.HttpContextItemKey));
            Assert.False(string.IsNullOrWhiteSpace(correlationAccessor.CorrelationId));
            return context.Response.WriteAsync("ok");
        });

        var pipeline = app.Build();
        var context = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        context.Features.Set<IHttpResponseFeature>(responseFeature);

        await pipeline(context);
        await responseFeature.ExecuteOnStartingAsync();

        Assert.Equal(correlationAccessor.CorrelationId, context.Items[CorrelationIdConstants.HttpContextItemKey]);
        Assert.Equal(correlationAccessor.CorrelationId, context.Response.Headers[CorrelationIdConstants.HeaderName].ToString());
    }

    [Fact]
    public async Task UseCorrelation_ShouldReuseIncomingHeaderValue()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCorrelation();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var correlationAccessor = scope.ServiceProvider.GetRequiredService<ICorrelationContextAccessor>();
        var app = new ApplicationBuilder(scope.ServiceProvider);
        var responseFeature = new TestHttpResponseFeature();
        const string correlationId = "corr-test-001";

        app.UseCorrelation();
        app.Run(context => context.Response.WriteAsync("ok"));

        var pipeline = app.Build();
        var context = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        context.Request.Headers[CorrelationIdConstants.HeaderName] = correlationId;

        await pipeline(context);
        await responseFeature.ExecuteOnStartingAsync();

        Assert.Equal(correlationId, correlationAccessor.CorrelationId);
        Assert.Equal(correlationId, context.Items[CorrelationIdConstants.HttpContextItemKey]);
        Assert.Equal(correlationId, context.Response.Headers[CorrelationIdConstants.HeaderName].ToString());
    }

    private sealed class TestHttpResponseFeature : IHttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> _onStartingCallbacks = [];

        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public string? ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = new MemoryStream();

        public bool HasStarted { get; private set; }

        public void OnStarting(Func<object, Task> callback, object state)
        {
            _onStartingCallbacks.Add((callback, state));
        }

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }

        public async Task ExecuteOnStartingAsync()
        {
            for (var index = _onStartingCallbacks.Count - 1; index >= 0; index--)
            {
                var callback = _onStartingCallbacks[index];
                await callback.Callback(callback.State);
            }

            HasStarted = true;
        }
    }
}

