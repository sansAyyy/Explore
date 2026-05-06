namespace Explore.AdminIdentityService.Application.Features.Authorization.Dtos.Responses;

public sealed record CurrentAdminAuthorizationResponse(
    Guid UserId,
    string UserName,
    string DisplayName,
    IReadOnlyCollection<string> RoleCodes,
    IReadOnlyCollection<string> PermissionCodes,
    IReadOnlyCollection<string> PagePermissionCodes,
    IReadOnlyCollection<string> ButtonPermissionCodes);

