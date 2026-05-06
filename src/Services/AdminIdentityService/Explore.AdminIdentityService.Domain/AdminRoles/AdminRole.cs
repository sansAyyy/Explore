using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Explore.AdminIdentityService.Domain.AdminRoles;

public sealed class AdminRole : AuditableEntity<Guid>
{
    private AdminRole()
    {
    }

    private AdminRole(
        Guid id,
        string code,
        string name,
        string? description,
        bool isActive)
    {
        Id = id;
        Apply(code, name, description);
        IsActive = isActive;
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public static AdminRole Create(
        Guid id,
        string code,
        string name,
        string? description,
        bool isActive)
    {
        return new AdminRole(id, code, name, description, isActive);
    }

    public void Update(
        string code,
        string name,
        string? description)
    {
        Apply(code, name, description);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private void Apply(string code, string name, string? description)
    {
        Code = RequireValue(code, nameof(code), 128);
        Name = RequireValue(name, nameof(name), 128);
        Description = NormalizeOptional(description, nameof(description), 256);
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

