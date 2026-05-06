using Explore.AdminIdentityService.Domain.AdminRoles;
using Explore.AdminIdentityService.Domain.AdminUsers;
using Explore.AdminIdentityService.Domain.AdminUserRoles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Configurations;

public sealed class AdminUserRoleConfiguration : IEntityTypeConfiguration<AdminUserRole>
{
    public void Configure(EntityTypeBuilder<AdminUserRole> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AdminUserId).IsRequired();
        builder.Property(x => x.AdminRoleId).IsRequired();

        builder.HasIndex(x => new { x.AdminUserId, x.AdminRoleId })
            .IsUnique();

        builder.HasOne<AdminUser>()
            .WithMany()
            .HasForeignKey(x => x.AdminUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AdminRole>()
            .WithMany()
            .HasForeignKey(x => x.AdminRoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

