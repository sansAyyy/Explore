using BuildingBlocks.Correlation.Abstractions.Constants;
using MassTransit;

namespace BuildingBlocks.Messaging.RabbitMQ.Resolvers
{
    internal static class CorrelationIdResolver
    {
        public static string Resolve<T>(ConsumeContext<T> context)
            where T : class
        {
            var correlationId = context.CorrelationId?.ToString()
                ?? GetHeaderValue(context.Headers);

            return string.IsNullOrWhiteSpace(correlationId)
                ? Guid.NewGuid().ToString("D")
                : correlationId;
        }

        public static string Resolve<T>(PublishContext<T> context, string? accessorCorrelationId)
            where T : class
        {
            var correlationId = context.CorrelationId?.ToString()
                ?? GetHeaderValue(context.Headers)
                ?? accessorCorrelationId;

            return string.IsNullOrWhiteSpace(correlationId)
                ? Guid.NewGuid().ToString("D")
                : correlationId;
        }

        private static string? GetHeaderValue(Headers headers)
        {
            return headers.TryGetHeader(CorrelationIdConstants.HeaderName, out var headerValue)
                ? headerValue?.ToString()
                : null;
        }
    }
}

