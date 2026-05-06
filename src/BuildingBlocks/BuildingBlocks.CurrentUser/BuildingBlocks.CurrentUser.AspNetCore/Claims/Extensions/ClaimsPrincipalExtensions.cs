using System.Security.Claims;

namespace BuildingBlocks.CurrentUser.AspNetCore.Claims.Extensions;

internal static class ClaimsPrincipalExtensions
{
    public static Guid? GetSubjectId(this ClaimsPrincipal? principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var subjectId) ? subjectId : null;
    }
}

