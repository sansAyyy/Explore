namespace BuildingBlocks.Security.PhoneVerification.Models;

public enum PhoneVerificationCodeVerificationStatus
{
    Verified = 1,
    MissingOrExpired = 2,
    Invalid = 3,
    AttemptsExceeded = 4
}

