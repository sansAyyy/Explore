using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Explore.CustomerAccountService.Domain.Customers;

public sealed class Customer : AuditableEntity<Guid>
{
    private Customer()
    {
    }

    private Customer(
        Guid id,
        string phoneNumber,
        string nickName,
        string? avatarUrl,
        string? email,
        bool isActive)
    {
        Id = id;
        ApplyProfile(phoneNumber, nickName, avatarUrl, email);
        IsActive = isActive;
    }

    public string PhoneNumber { get; private set; } = string.Empty;

    public string NickName { get; private set; } = string.Empty;

    public string? AvatarUrl { get; private set; }

    public string? Email { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public static Customer Create(
        Guid id,
        string phoneNumber,
        string nickName,
        string? avatarUrl,
        string? email,
        bool isActive)
    {
        return new Customer(id, phoneNumber, nickName, avatarUrl, email, isActive);
    }

    public void UpdateProfile(string nickName, string? avatarUrl, string? email)
    {
        NickName = RequireValue(nickName, nameof(nickName), 64);
        AvatarUrl = NormalizeOptionalValue(avatarUrl, nameof(avatarUrl), 512);
        Email = NormalizeOptionalValue(email, nameof(email), 256);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void MarkLogin(DateTime lastLoginAt)
    {
        LastLoginAt = lastLoginAt;
    }

    private void ApplyProfile(string phoneNumber, string nickName, string? avatarUrl, string? email)
    {
        PhoneNumber = RequireValue(phoneNumber, nameof(phoneNumber), 32);
        NickName = RequireValue(nickName, nameof(nickName), 64);
        AvatarUrl = NormalizeOptionalValue(avatarUrl, nameof(avatarUrl), 512);
        Email = NormalizeOptionalValue(email, nameof(email), 256);
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

    private static string? NormalizeOptionalValue(string? value, string fieldName, int maxLength)
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

