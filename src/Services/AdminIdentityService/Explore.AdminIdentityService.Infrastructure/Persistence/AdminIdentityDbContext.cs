using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Extensions;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Models;
using BuildingBlocks.Persistence.Auditing.Abstractions;
using BuildingBlocks.Persistence.Auditing.EntityFrameworkCore.Auditing;
using Microsoft.EntityFrameworkCore;
using Explore.AdminIdentityService.Domain.AdminPermissions;
using Explore.AdminIdentityService.Domain.AdminRolePermissions;
using Explore.AdminIdentityService.Domain.AdminRoles;
using Explore.AdminIdentityService.Domain.AdminUsers;
using Explore.AdminIdentityService.Domain.AdminUserRoles;

namespace Explore.AdminIdentityService.Infrastructure.Persistence;

public sealed class AdminIdentityDbContext : AuditableDbContext
{
    public AdminIdentityDbContext(
        DbContextOptions<AdminIdentityDbContext> options,
        IAuditActorAccessor auditActorAccessor)
        : base(options, auditActorAccessor)
    {
    }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AdminRole> AdminRoles => Set<AdminRole>();
    public DbSet<AdminPermission> AdminPermissions => Set<AdminPermission>();
    public DbSet<AdminUserRole> AdminUserRoles => Set<AdminUserRole>();
    public DbSet<AdminRolePermission> AdminRolePermissions => Set<AdminRolePermission>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdminIdentityDbContext).Assembly);
    }
}


