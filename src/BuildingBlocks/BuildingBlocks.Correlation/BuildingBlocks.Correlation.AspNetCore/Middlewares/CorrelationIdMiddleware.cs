using BuildingBlocks.Correlation.Abstractions.Constants;
using BuildingBlocks.Correlation.Abstractions.ContextAccessors;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace BuildingBlocks.Correlation.AspNetCore.Middlewares
{
    internal class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ICorrelationContextAccessor correlationAccessor)
        {
            var correlationId = context.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("D");
            }

            context.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId;
            correlationAccessor.CorrelationId = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdConstants.HeaderName] = correlationId;
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty(CorrelationIdConstants.LogPropertyName, correlationId))
            {
                await _next(context);
            }
        }
    }
}

