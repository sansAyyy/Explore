using Explore.CustomerAccountService.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Explore.CustomerAccountService.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PhoneNumber).HasColumnType("citext").HasMaxLength(32).IsRequired();
        builder.Property(x => x.NickName).HasMaxLength(64).IsRequired();
        builder.Property(x => x.AvatarUrl).HasMaxLength(512);
        builder.Property(x => x.Email).HasColumnType("citext").HasMaxLength(256);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.LastLoginAt);

        builder.HasIndex(x => x.PhoneNumber).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
    }
}

