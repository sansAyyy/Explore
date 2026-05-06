using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Explore.AdminIdentityService.Domain.AdminUserRoles;

public sealed class AdminUserRole : AuditableEntity<Guid>
{
    private AdminUserRole()
    {
    }

    private AdminUserRole(Guid id, Guid adminUserId, Guid adminRoleId)
    {
        Id = id;
        AdminUserId = RequireId(adminUserId, nameof(adminUserId));
        AdminRoleId = RequireId(adminRoleId, nameof(adminRoleId));
    }

    public Guid AdminUserId { get; private set; }

    public Guid AdminRoleId { get; private set; }

    public static AdminUserRole Create(Guid id, Guid adminUserId, Guid adminRoleId)
    {
        return new AdminUserRole(id, adminUserId, adminRoleId);
    }

    private static Guid RequireId(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException($"{fieldName} is required.");
        }

        return value;
    }
}

