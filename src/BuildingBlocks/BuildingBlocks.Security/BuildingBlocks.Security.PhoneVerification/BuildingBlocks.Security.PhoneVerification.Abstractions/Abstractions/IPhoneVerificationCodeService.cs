using BuildingBlocks.Common.Results;
using BuildingBlocks.Security.PhoneVerification.Models;

namespace BuildingBlocks.Security.PhoneVerification.Abstractions;

public interface IPhoneVerificationCodeService
{
    Task<Result<PhoneVerificationCodeIssueResult>> IssueAsync(
        string scope,
        string subject,
        CancellationToken cancellationToken);

    Task<PhoneVerificationCodeVerificationStatus> VerifyAsync(
        string scope,
        string subject,
        string submittedCode,
        CancellationToken cancellationToken);

    Task InvalidateAsync(
        string scope,
        string subject,
        CancellationToken cancellationToken);
}

