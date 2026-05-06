using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Explore.AdminIdentityService.Domain.AdminUsers;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Configurations;

public sealed class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserName).HasColumnType("citext").HasMaxLength(64).IsRequired();
        builder.Property(x => x.Email).HasColumnType("citext").HasMaxLength(256).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.PhoneNumber).HasColumnType("citext").HasMaxLength(32);
        builder.Property(x => x.PasswordHash).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.LastLoginAt);

        builder.HasIndex(x => x.UserName)
            .IsUnique();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasIndex(x => x.PhoneNumber)
            .IsUnique();
    }
}

