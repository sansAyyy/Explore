using System.Net;
using System.Text.Json;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace BuildingBlocks.Common.Http;

public static class OutboundHttpFailureMapper
{
    public static async Task<Error> CreateErrorAsync(
        HttpResponseMessage response,
        string dependencyName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(response);

        var message = await TryReadProblemTitleAsync(response, cancellationToken)
            ?? TryCreateStatusSpecificMessage(response.StatusCode, dependencyName)
            ?? $"{dependencyName} request failed with status code {(int)response.StatusCode}.";

        return Error.BadRequest(message);
    }

    public static Error CreateError(Exception exception, string dependencyName)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            TimeoutRejectedException => Error.BadRequest($"{dependencyName} request timed out."),
            BrokenCircuitException => Error.BadRequest($"{dependencyName} is temporarily unavailable."),
            HttpRequestException => Error.BadRequest($"{dependencyName} request failed."),
            OperationCanceledException operationCanceledException
                when operationCanceledException.CancellationToken == CancellationToken.None =>
                Error.BadRequest($"{dependencyName} request timed out."),
            _ => Error.BadRequest($"{dependencyName} request failed.")
        };
    }

    private static string? TryCreateStatusSpecificMessage(HttpStatusCode statusCode, string dependencyName)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound => $"{dependencyName} resource was not found.",
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => $"{dependencyName} request was rejected.",
            HttpStatusCode.TooManyRequests => $"{dependencyName} is throttling requests.",
            _ => null
        };
    }

    private static async Task<string?> TryReadProblemTitleAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return content.Trim();
            }

            if (document.RootElement.TryGetProperty("title", out var title) &&
                title.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(title.GetString()))
            {
                return title.GetString()!.Trim();
            }
        }
        catch (JsonException)
        {
            return content.Trim();
        }

        return content.Trim();
    }
}

