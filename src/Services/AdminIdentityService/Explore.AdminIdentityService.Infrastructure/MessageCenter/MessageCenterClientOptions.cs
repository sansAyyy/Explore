namespace Explore.AdminIdentityService.Infrastructure.MessageCenter;

public sealed class MessageCenterClientOptions
{
    public const string SectionName = "MessageCenter";

    public string BaseUrl { get; set; } = string.Empty;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}

