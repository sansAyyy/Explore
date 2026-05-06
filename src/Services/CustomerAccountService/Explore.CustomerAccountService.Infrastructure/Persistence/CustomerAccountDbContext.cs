using BuildingBlocks.Persistence.Auditing.Abstractions;
using BuildingBlocks.Persistence.Auditing.EntityFrameworkCore.Auditing;
using Explore.CustomerAccountService.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Explore.CustomerAccountService.Infrastructure.Persistence;

public sealed class CustomerAccountDbContext : AuditableDbContext
{
    public CustomerAccountDbContext(
        DbContextOptions<CustomerAccountDbContext> options,
        IAuditActorAccessor auditActorAccessor)
        : base(options, auditActorAccessor)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerAccountDbContext).Assembly);
    }
}

