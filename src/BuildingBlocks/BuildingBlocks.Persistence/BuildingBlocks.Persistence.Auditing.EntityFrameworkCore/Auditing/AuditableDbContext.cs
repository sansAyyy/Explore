using System.Linq.Expressions;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Persistence.Auditing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BuildingBlocks.Persistence.Auditing.EntityFrameworkCore.Auditing;

public abstract class AuditableDbContext : DbContext
{
    private readonly IAuditActorAccessor _auditActorAccessor;

    protected AuditableDbContext(
        DbContextOptions options,
        IAuditActorAccessor auditActorAccessor)
        : base(options)
    {
        _auditActorAccessor = auditActorAccessor;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditing();
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplyAuditableEntityConfiguration(modelBuilder);
    }

    private void ApplyAuditing()
    {
        ChangeTracker.DetectChanges();

        var now = DateTime.UtcNow;
        var actor = _auditActorAccessor.AuditActor;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                entry.Property(x => x.CreatedAt).CurrentValue = now;
                entry.Property(x => x.CreatedBy).CurrentValue = actor;
                entry.Property(x => x.UpdatedAt).CurrentValue = null;
                entry.Property(x => x.UpdatedBy).CurrentValue = null;
                entry.Property(x => x.IsDeleted).CurrentValue = false;
                entry.Property(x => x.Version).CurrentValue = 1;
                continue;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Property(x => x.IsDeleted).CurrentValue = true;
            }

            entry.Property(x => x.CreatedAt).IsModified = false;
            entry.Property(x => x.CreatedBy).IsModified = false;
            entry.Property(x => x.UpdatedAt).CurrentValue = now;
            entry.Property(x => x.UpdatedBy).CurrentValue = actor;
            entry.Property(x => x.Version).CurrentValue = GetNextVersion(entry);
        }
    }

    private static long GetNextVersion(EntityEntry<IAuditableEntity> entry)
    {
        var currentVersion = entry.Property(x => x.Version).CurrentValue;
        return currentVersion <= 0 ? 1 : currentVersion + 1;
    }

    private static void ApplyAuditableEntityConfiguration(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(x => typeof(IAuditableEntity).IsAssignableFrom(x.ClrType)))
        {
            var entityBuilder = modelBuilder.Entity(entityType.ClrType);

            entityBuilder.Property<DateTime>(nameof(IAuditableEntity.CreatedAt)).IsRequired();
            entityBuilder.Property<string>(nameof(IAuditableEntity.CreatedBy)).HasMaxLength(256).IsRequired();
            entityBuilder.Property<DateTime?>(nameof(IAuditableEntity.UpdatedAt));
            entityBuilder.Property<string?>(nameof(IAuditableEntity.UpdatedBy)).HasMaxLength(256);
            entityBuilder.Property<bool>(nameof(IAuditableEntity.IsDeleted)).IsRequired();
            entityBuilder.Property<long>(nameof(IAuditableEntity.Version)).IsRequired().IsConcurrencyToken();
            entityBuilder.HasQueryFilter(CreateNotDeletedFilter(entityType.ClrType));
        }
    }

    private static LambdaExpression CreateNotDeletedFilter(Type entityClrType)
    {
        var parameter = Expression.Parameter(entityClrType, "entity");
        var isDeleted = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            [typeof(bool)],
            parameter,
            Expression.Constant(nameof(IAuditableEntity.IsDeleted)));

        var predicate = Expression.Equal(isDeleted, Expression.Constant(false));
        return Expression.Lambda(predicate, parameter);
    }
}
