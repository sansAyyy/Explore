using BuildingBlocks.Common.Results;
using Explore.AdminIdentityService.Application.Features.UserRoles.Dtos.Requests;

namespace Explore.AdminIdentityService.Application.Features.UserRoles.Validators;

internal static class AdminUserRoleRequestValidator
{
    public static Error? Validate(AssignUserRolesRequest request)
    {
        if (request.RoleIds.Any(x => x == Guid.Empty))
        {
            return Error.Validation("RoleIds contains invalid id.");
        }

        return null;
    }
}

