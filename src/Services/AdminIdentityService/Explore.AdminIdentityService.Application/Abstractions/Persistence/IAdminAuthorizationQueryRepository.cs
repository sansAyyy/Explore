using Explore.AdminIdentityService.Application.Features.Authorization.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Abstractions.Persistence;

public interface IAdminAuthorizationQueryRepository
{
    Task<CurrentAdminAuthorizationResponse?> GetCurrentAsync(Guid userId, CancellationToken cancellationToken);
}

