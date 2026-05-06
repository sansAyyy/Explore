using Explore.AdminIdentityService.Domain.AdminPermissions;
using Explore.AdminIdentityService.Domain.AdminRolePermissions;
using Explore.AdminIdentityService.Domain.AdminRoles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Configurations;

public sealed class AdminRolePermissionConfiguration : IEntityTypeConfiguration<AdminRolePermission>
{
    public void Configure(EntityTypeBuilder<AdminRolePermission> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AdminRoleId).IsRequired();
        builder.Property(x => x.AdminPermissionId).IsRequired();

        builder.HasIndex(x => new { x.AdminRoleId, x.AdminPermissionId })
            .IsUnique();

        builder.HasOne<AdminRole>()
            .WithMany()
            .HasForeignKey(x => x.AdminRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AdminPermission>()
            .WithMany()
            .HasForeignKey(x => x.AdminPermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

