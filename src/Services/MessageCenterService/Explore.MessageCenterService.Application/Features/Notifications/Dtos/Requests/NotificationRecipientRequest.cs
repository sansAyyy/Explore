namespace Explore.MessageCenterService.Application.Features.Notifications.Dtos.Requests;

public sealed class NotificationRecipientRequest
{
    public Guid? UserId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? MiniProgramOpenId { get; set; }
}

