using BuildingBlocks.Common.Pagination;
using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Features.AdminUsers.Abstractions;

public interface IAdminUserAppService
{
    Task<Result<PagedResult<AdminUserBasicResponse>>> GetPagedAsync(GetPagedAdminUsersRequest request, CancellationToken cancellationToken);

    Task<Result<AdminUserDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<AdminUserDetailResponse>> CreateAsync(CreateAdminUserRequest request, CancellationToken cancellationToken);

    Task<Result<AdminUserDetailResponse>> UpdateAsync(Guid id, UpdateAdminUserRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> ChangePasswordAsync(Guid id, ChangeAdminUserPasswordRequest request, CancellationToken cancellationToken);

    Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken);
}

