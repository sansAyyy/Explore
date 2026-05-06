namespace BuildingBlocks.Security.PhoneVerification.Options;

public sealed class PhoneVerificationCodeOptions
{
    public const string SectionName = "PhoneVerificationCode";

    public int CodeLength { get; set; } = 6;

    public TimeSpan CodeTtl { get; set; } = TimeSpan.FromMinutes(5);

    public TimeSpan SendInterval { get; set; } = TimeSpan.FromMinutes(1);

    public int MaxVerifyAttempts { get; set; } = 5;
}

