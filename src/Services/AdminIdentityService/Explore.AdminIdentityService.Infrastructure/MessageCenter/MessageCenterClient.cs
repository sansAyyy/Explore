using System.Net.Http.Json;
using BuildingBlocks.Common.Http;
using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Abstractions.Notifications;

namespace Explore.AdminIdentityService.Infrastructure.MessageCenter;

public sealed class MessageCenterClient : IAdminMessageCenterClient
{
    private const int SmsChannel = 1;
    private static readonly Uri SendNotificationPath = new("api/notifications/send", UriKind.Relative);
    private readonly HttpClient _httpClient;

    public MessageCenterClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result> SendPhoneLoginCodeAsync(
        string phoneNumber,
        string verificationCode,
        TimeSpan expiresIn,
        CancellationToken cancellationToken)
    {
        var request = new SendNotificationHttpRequest
        {
            TemplateCode = AdminIdentitySmsTemplateCodes.PhoneLoginCode,
            Channel = SmsChannel,
            Recipient = new NotificationRecipientHttpRequest
            {
                PhoneNumber = phoneNumber.Trim()
            },
            Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["code"] = verificationCode.Trim(),
                ["expireMinutes"] = Math.Max(1, (int)Math.Ceiling(expiresIn.TotalMinutes)).ToString()
            }
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                SendNotificationPath,
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            return Result.Failure(await OutboundHttpFailureMapper.CreateErrorAsync(
                response,
                "Message center",
                cancellationToken));
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return Result.Failure(OutboundHttpFailureMapper.CreateError(exception, "Message center"));
        }
    }

    private sealed class SendNotificationHttpRequest
    {
        public string TemplateCode { get; set; } = string.Empty;

        public int Channel { get; set; }

        public NotificationRecipientHttpRequest? Recipient { get; set; }

        public Dictionary<string, string> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class NotificationRecipientHttpRequest
    {
        public Guid? UserId { get; set; }

        public string? PhoneNumber { get; set; }

        public string? MiniProgramOpenId { get; set; }
    }
}

