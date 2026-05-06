using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Explore.AdminIdentityService.Domain.AdminUsers
{
    public class AdminUser : AuditableEntity<Guid>
    {
        private AdminUser()
        {
        }

        private AdminUser(
            Guid id,
            string userName,
            string email,
            string displayName,
            string? phoneNumber,
            string passwordHash,
            bool isActive)
        {
            Id = id;
            ApplyProfile(userName, email, displayName, phoneNumber);
            PasswordHash = RequireValue(passwordHash, nameof(passwordHash), 2048);
            IsActive = isActive;
        }

        public string UserName { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string DisplayName { get; private set; } = string.Empty;

        public string? PhoneNumber { get; private set; }

        public string PasswordHash { get; private set; } = string.Empty;

        public bool IsActive { get; private set; }

        public DateTime? LastLoginAt { get; private set; }

        public static AdminUser Create(
            Guid id,
            string userName,
            string email,
            string displayName,
            string? phoneNumber,
            string passwordHash,
            bool isActive)
        {
            return new AdminUser(id, userName, email, displayName, phoneNumber, passwordHash, isActive);
        }

        public void Update(
            string userName,
            string email,
            string displayName,
            string? phoneNumber,
            bool isActive)
        {
            UpdateProfile(userName, email, displayName, phoneNumber);

            if (isActive)
            {
                Activate();
                return;
            }

            Deactivate();
        }

        public void UpdateProfile(
            string userName,
            string email,
            string displayName,
            string? phoneNumber)
        {
            ApplyProfile(userName, email, displayName, phoneNumber);
        }

        public void ChangePassword(string passwordHash)
        {
            PasswordHash = RequireValue(passwordHash, nameof(passwordHash), 2048);
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

        private void ApplyProfile(string userName, string email, string displayName, string? phoneNumber)
        {
            UserName = RequireValue(userName, nameof(userName), 64);
            Email = RequireValue(email, nameof(email), 256);
            DisplayName = RequireValue(displayName, nameof(displayName), 128);
            PhoneNumber = NormalizeOptionalValue(phoneNumber, nameof(phoneNumber), 32);
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
}

