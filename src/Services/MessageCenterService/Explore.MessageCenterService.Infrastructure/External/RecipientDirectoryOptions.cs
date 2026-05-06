namespace Explore.MessageCenterService.Infrastructure.External;

public sealed class RecipientDirectoryOptions
{
    public const string SectionName = "RecipientDirectoryOptions";

    public string BaseUrl { get; set; } = string.Empty;

    public string ProfilePathTemplate { get; set; } = "/api/users/{userId}/notification-profile";

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}

