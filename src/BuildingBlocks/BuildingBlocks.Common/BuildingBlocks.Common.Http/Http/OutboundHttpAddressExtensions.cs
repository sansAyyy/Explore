namespace BuildingBlocks.Common.Http;

public static class OutboundHttpAddressExtensions
{
    public static Uri ToRequiredAbsoluteUri(this string? baseUrl, string settingName)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseAddress))
        {
            throw new InvalidOperationException($"{settingName} must be an absolute URI.");
        }

        return baseAddress;
    }
}

