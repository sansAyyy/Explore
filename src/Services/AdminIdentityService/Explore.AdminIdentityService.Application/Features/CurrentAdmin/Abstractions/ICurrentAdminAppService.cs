using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.CurrentAdmin.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.CurrentAdmin.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Features.CurrentAdmin.Abstractions;

public interface ICurrentAdminAppService
{
    Task<Result<CurrentAdminResponse>> GetCurrentAsync(CancellationToken cancellationToken);

    Task<Result<CurrentAdminResponse>> UpdateProfileAsync(UpdateCurrentAdminProfileRequest request, CancellationToken cancellationToken);

    Task<Result> ChangePasswordAsync(ChangeCurrentAdminPasswordRequest request, CancellationToken cancellationToken);
}

