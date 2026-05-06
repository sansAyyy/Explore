using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Common.Http;
using BuildingBlocks.Common.Results;
using Explore.MessageCenterService.Application.Abstractions.External;
using Microsoft.Extensions.Options;

namespace Explore.MessageCenterService.Infrastructure.External;

public sealed class HttpRecipientDirectoryClient : IRecipientDirectoryClient
{
    private readonly HttpClient _httpClient;
    private readonly RecipientDirectoryOptions _options;

    public HttpRecipientDirectoryClient(HttpClient httpClient, IOptions<RecipientDirectoryOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<Result<RecipientProfileDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var requestPath = _options.ProfilePathTemplate.Replace("{userId}", userId.ToString(), StringComparison.OrdinalIgnoreCase);

        try
        {
            using var response = await _httpClient.GetAsync(requestPath, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.Failure<RecipientProfileDto>(Error.NotFound($"Recipient profile '{userId}' was not found."));
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<RecipientProfileDto>(await OutboundHttpFailureMapper.CreateErrorAsync(
                    response,
                    "Recipient directory",
                    cancellationToken));
            }

            var profile = await response.Content.ReadFromJsonAsync<RecipientProfileDto>(cancellationToken: cancellationToken);
            return profile is null
                ? Result.Failure<RecipientProfileDto>(Error.BadRequest("Recipient directory returned an empty profile payload."))
                : Result.Success(profile);
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<RecipientProfileDto>(OutboundHttpFailureMapper.CreateError(exception, "Recipient directory"));
        }
    }
}

