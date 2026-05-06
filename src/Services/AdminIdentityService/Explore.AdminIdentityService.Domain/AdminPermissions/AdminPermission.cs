using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Explore.AdminIdentityService.Domain.AdminPermissions;

public sealed class AdminPermission : AuditableEntity<Guid>
{
    private AdminPermission()
    {
    }

    private AdminPermission(
        Guid id,
        Guid? parentId,
        string code,
        string name,
        string? description,
        PermissionResourceType resourceType,
        bool isActive)
    {
        Id = id;
        Apply(parentId, code, name, description, resourceType);
        IsActive = isActive;
    }

    public Guid? ParentId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public PermissionResourceType ResourceType { get; private set; }

    public bool IsActive { get; private set; }

    public static AdminPermission Create(
        Guid id,
        Guid? parentId,
        string code,
        string name,
        string? description,
        PermissionResourceType resourceType,
        bool isActive)
    {
        return new AdminPermission(id, parentId, code, name, description, resourceType, isActive);
    }

    public void Update(
        Guid? parentId,
        string code,
        string name,
        string? description,
        PermissionResourceType resourceType)
    {
        Apply(parentId, code, name, description, resourceType);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private void Apply(
        Guid? parentId,
        string code,
        string name,
        string? description,
        PermissionResourceType resourceType)
    {
        if (parentId.HasValue && parentId.Value == Id)
        {
            throw new DomainException($"{nameof(ParentId)} cannot reference itself.");
        }

        ParentId = parentId;
        Code = RequireValue(code, nameof(code), 128);
        Name = RequireValue(name, nameof(name), 128);
        Description = NormalizeOptional(description, nameof(description), 256);
        if (resourceType is not PermissionResourceType.Page and not PermissionResourceType.Button and not PermissionResourceType.Group)
        {
            throw new DomainException($"{nameof(resourceType)} is invalid.");
        }

        ResourceType = resourceType;
    }

    private static string RequireValue(string value, string fieldName, int maxLength)
    {
        var trimmedValue = value.Trim();

        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        if (trimmedValue.Length > maxLength)
        {
            throw new DomainException($"{fieldName} exceeds max length {maxLength}.");
        }

        return trimmedValue;
    }

    private static string? NormalizeOptional(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmedValue = value.Trim();
        if (trimmedValue.Length > maxLength)
        {
            throw new DomainException($"{fieldName} exceeds max length {maxLength}.");
        }

        return trimmedValue;
    }
}

