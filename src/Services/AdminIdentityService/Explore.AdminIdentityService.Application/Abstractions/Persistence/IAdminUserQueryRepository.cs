using BuildingBlocks.Common.Pagination;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Requests;
using Explore.AdminIdentityService.Application.Features.AdminUsers.Dtos.Responses;

namespace Explore.AdminIdentityService.Application.Abstractions.Persistence;

public interface IAdminUserQueryRepository
{
    Task<AdminUserDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<AdminUserBasicResponse>> GetPagedAsync(GetPagedAdminUsersRequest request, CancellationToken cancellationToken);
}

