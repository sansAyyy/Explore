using Explore.AdminIdentityService.Domain.AdminPermissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.AdminIdentityService.Infrastructure.Persistence.Configurations;

public sealed class AdminPermissionConfiguration : IEntityTypeConfiguration<AdminPermission>
{
    public void Configure(EntityTypeBuilder<AdminPermission> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasColumnType("citext").HasMaxLength(128).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(256);
        builder.Property(x => x.ResourceType).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.ParentId);

        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasOne<AdminPermission>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

