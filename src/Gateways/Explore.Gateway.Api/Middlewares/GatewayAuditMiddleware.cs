using System.Diagnostics;
using System.Security.Claims;
using BuildingBlocks.Correlation.Abstractions.Constants;
using BuildingBlocks.Security.Authentication.Constants;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Model;

namespace Explore.Gateway.Api.Middlewares;

public sealed class GatewayAuditMiddleware
{
    private const string AuditType = "GatewayRequest";

    private readonly RequestDelegate _next;
    private readonly ILogger<GatewayAuditMiddleware> _logger;

    public GatewayAuditMiddleware(
        RequestDelegate next,
        ILogger<GatewayAuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            await _next(context);
        }
        finally
        {
            var statusCode = context.Response.StatusCode;
            var proxyFeature = context.Features.Get<IReverseProxyFeature>();
            var forwarderErrorFeature = context.Features.Get<IForwarderErrorFeature>();
            var elapsed = Stopwatch.GetElapsedTime(startedAt);
            var outcome = ResolveOutcome(statusCode, forwarderErrorFeature);

            _logger.Log(
                ResolveLogLevel(statusCode, forwarderErrorFeature),
                "Gateway audit completed AuditType={AuditType} Outcome={Outcome} Method={Method} Path={Path} StatusCode={StatusCode} DurationMs={DurationMs} RouteId={RouteId} ClusterId={ClusterId} UserId={UserId} TokenParty={TokenParty} IsAuthenticated={IsAuthenticated} ClientIp={ClientIp} CorrelationId={CorrelationId} TraceId={TraceId} SpanId={SpanId} ProxyError={ProxyError}",
                AuditType,
                outcome,
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                Math.Round(elapsed.TotalMilliseconds, 3),
                ResolveRouteId(proxyFeature),
                ResolveClusterId(proxyFeature),
                ResolveUserId(context),
                ResolveTokenParty(context),
                context.User.Identity?.IsAuthenticated ?? false,
                context.Connection.RemoteIpAddress?.ToString(),
                ResolveCorrelationId(context),
                Activity.Current?.TraceId.ToString(),
                Activity.Current?.SpanId.ToString(),
                ResolveProxyError(forwarderErrorFeature));
        }
    }

    private static string ResolveOutcome(
        int statusCode,
        IForwarderErrorFeature? forwarderErrorFeature)
    {
        if (forwarderErrorFeature is not null && forwarderErrorFeature.Error != ForwarderError.None)
        {
            return "ProxyFailed";
        }

        return statusCode switch
        {
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status429TooManyRequests => "RateLimited",
            >= 500 => "DownstreamFailed",
            >= 400 => "Rejected",
            _ => "Succeeded"
        };
    }

    private static LogLevel ResolveLogLevel(
        int statusCode,
        IForwarderErrorFeature? forwarderErrorFeature)
    {
        if (forwarderErrorFeature is not null && forwarderErrorFeature.Error != ForwarderError.None)
        {
            return LogLevel.Error;
        }

        if (statusCode >= 500)
        {
            return LogLevel.Error;
        }

        if (statusCode >= 400)
        {
            return LogLevel.Warning;
        }

        return LogLevel.Information;
    }

    private static string? ResolveUserId(HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private static string? ResolveTokenParty(HttpContext context)
    {
        return context.User.FindFirst(SecurityClaimNames.TokenParty)?.Value;
    }

    private static string? ResolveCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdConstants.HttpContextItemKey, out var correlationId))
        {
            return correlationId?.ToString();
        }

        return context.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault();
    }

    private static string? ResolveRouteId(IReverseProxyFeature? proxyFeature)
    {
        return TryGetStringProperty(proxyFeature?.Route, "RouteId")
            ?? TryGetNestedStringProperty(proxyFeature?.Route, "Config", "RouteId");
    }

    private static string? ResolveClusterId(IReverseProxyFeature? proxyFeature)
    {
        return TryGetStringProperty(proxyFeature?.Cluster, "ClusterId")
            ?? TryGetNestedStringProperty(proxyFeature?.Cluster, "Config", "ClusterId")
            ?? TryGetNestedStringProperty(proxyFeature?.Route, "Config", "ClusterId");
    }

    private static string? ResolveProxyError(IForwarderErrorFeature? forwarderErrorFeature)
    {
        if (forwarderErrorFeature is null || forwarderErrorFeature.Error == ForwarderError.None)
        {
            return null;
        }

        return forwarderErrorFeature.Error.ToString();
    }

    private static string? TryGetNestedStringProperty(
        object? source,
        string parentPropertyName,
        string childPropertyName)
    {
        var parent = source?.GetType().GetProperty(parentPropertyName)?.GetValue(source);
        return TryGetStringProperty(parent, childPropertyName);
    }

    private static string? TryGetStringProperty(object? source, string propertyName)
    {
        return source?.GetType().GetProperty(propertyName)?.GetValue(source)?.ToString();
    }
}

