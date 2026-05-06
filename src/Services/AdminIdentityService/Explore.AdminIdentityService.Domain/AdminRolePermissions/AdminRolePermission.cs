using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Explore.AdminIdentityService.Domain.AdminRolePermissions;

public sealed class AdminRolePermission : AuditableEntity<Guid>
{
    private AdminRolePermission()
    {
    }

    private AdminRolePermission(Guid id, Guid adminRoleId, Guid adminPermissionId)
    {
        Id = id;
        AdminRoleId = RequireId(adminRoleId, nameof(adminRoleId));
        AdminPermissionId = RequireId(adminPermissionId, nameof(adminPermissionId));
    }

    public Guid AdminRoleId { get; private set; }

    public Guid AdminPermissionId { get; private set; }

    public static AdminRolePermission Create(Guid id, Guid adminRoleId, Guid adminPermissionId)
    {
        return new AdminRolePermission(id, adminRoleId, adminPermissionId);
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

