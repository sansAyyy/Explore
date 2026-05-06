namespace BuildingBlocks.OpenApi.AspNetCore.Options;

public sealed class ServiceOpenApiOptions
{
    private bool? _enablePersistentAuthentication;

    public string Title { get; set; } = string.Empty;

    public string Version { get; set; } = "v1";

    public string? Description { get; set; }

    public bool UseBearerSecurity { get; set; }

    public string BearerSchemeName { get; set; } = "Bearer";

    public string BearerDescription { get; set; } = "JWT Bearer token for protected APIs.";

    public bool EnablePersistentAuthentication
    {
        get => _enablePersistentAuthentication ?? UseBearerSecurity;
        set => _enablePersistentAuthentication = value;
    }
}

