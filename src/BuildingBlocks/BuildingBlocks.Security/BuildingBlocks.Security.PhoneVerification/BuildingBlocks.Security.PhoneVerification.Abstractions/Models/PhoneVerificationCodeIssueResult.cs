namespace BuildingBlocks.Security.PhoneVerification.Models;

public sealed record PhoneVerificationCodeIssueResult(
    string Code,
    DateTime ExpiresAt);

