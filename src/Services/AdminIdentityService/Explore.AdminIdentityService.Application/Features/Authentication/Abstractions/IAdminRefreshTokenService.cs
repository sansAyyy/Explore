using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.Authentication.Dtos.Models;

namespace Explore.AdminIdentityService.Application.Features.Authentication.Abstractions;

public interface IAdminRefreshTokenService
{
    Task<RefreshTokenIssueResult> CreateAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<ValidatedRefreshTokenSession>> ValidateAsync(string refreshToken, CancellationToken cancellationToken);

    Task<RefreshTokenIssueResult> RotateAsync(ValidatedRefreshTokenSession session, CancellationToken cancellationToken);

    Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken);
}

