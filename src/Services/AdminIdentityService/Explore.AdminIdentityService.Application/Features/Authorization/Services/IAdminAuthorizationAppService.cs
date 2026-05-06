using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.Authorization.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Features.Authorization.Services;

public interface IAdminAuthorizationAppService
{
    Task<Result<CurrentAdminAuthorizationResponse>> GetCurrentAsync(CancellationToken cancellationToken);
}

