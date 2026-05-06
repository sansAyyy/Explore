using BuildingBlocks.CurrentUser.Abstractions;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Persistence.Auditing.Abstractions;
using BuildingBlocks.Persistence.Auditing.CurrentUser.Extensions;
using BuildingBlocks.Persistence.Auditing.CurrentUser.Services;
using BuildingBlocks.Persistence.Auditing.EntityFrameworkCore.Auditing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class PersistenceAuditingTests
{
    [Fact]
    public void AddCurrentUserAuditActorAccessor_ShouldRegisterAuditActorAccessor()
    {
        var services = new ServiceCollection();
        services.AddScoped<ICurrentUser, FakeCurrentUser>();

        services.AddCurrentUserAuditActorAccessor();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var accessor = scope.ServiceProvider.GetRequiredService<IAuditActorAccessor>();

        Assert.IsType<CurrentUserAuditActorAccessor>(accessor);
        Assert.Equal(FakeCurrentUser.Actor, accessor.AuditActor);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPopulateAuditingFields_ForAddedEntity()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var dbContext = CreateDbContext(connection);
        await dbContext.Database.EnsureCreatedAsync();

        var entity = TestAuditableEntity.Create("created");
        dbContext.Entities.Add(entity);

        await dbContext.SaveChangesAsync();

        Assert.NotEqual(default, entity.CreatedAt);
        Assert.Equal("actor-001", entity.CreatedBy);
        Assert.Null(entity.UpdatedAt);
        Assert.Null(entity.UpdatedBy);
        Assert.False(entity.IsDeleted);
        Assert.Equal(1, entity.Version);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldUpdateAuditingFieldsAndIncrementVersion_ForModifiedEntity()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var dbContext = CreateDbContext(connection);
        await dbContext.Database.EnsureCreatedAsync();

        var entity = TestAuditableEntity.Create("created");
        dbContext.Entities.Add(entity);
        await dbContext.SaveChangesAsync();

        var createdAt = entity.CreatedAt;
        var createdBy = entity.CreatedBy;

        entity.Rename("updated");
        await dbContext.SaveChangesAsync();

        Assert.Equal(createdAt, entity.CreatedAt);
        Assert.Equal(createdBy, entity.CreatedBy);
        Assert.NotNull(entity.UpdatedAt);
        Assert.Equal("actor-001", entity.UpdatedBy);
        Assert.Equal(2, entity.Version);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSoftDeleteEntity_AndQueryFilterShouldHideIt()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var dbContext = CreateDbContext(connection);
        await dbContext.Database.EnsureCreatedAsync();

        var entity = TestAuditableEntity.Create("created");
        dbContext.Entities.Add(entity);
        await dbContext.SaveChangesAsync();

        dbContext.Entities.Remove(entity);
        await dbContext.SaveChangesAsync();

        Assert.Empty(await dbContext.Entities.ToListAsync());

        var deletedEntity = await dbContext.Entities
            .IgnoreQueryFilters()
            .SingleAsync();

        Assert.True(deletedEntity.IsDeleted);
        Assert.NotNull(deletedEntity.UpdatedAt);
        Assert.Equal("actor-001", deletedEntity.UpdatedBy);
        Assert.Equal(2, deletedEntity.Version);
    }

    [Fact]
    public void OnModelCreating_ShouldConfigureConcurrencyTokenAndQueryFilter()
    {
        var options = new DbContextOptionsBuilder<TestAuditableDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        using var dbContext = new TestAuditableDbContext(options, new FakeAuditActorAccessor());

        var entityType = dbContext.Model.FindEntityType(typeof(TestAuditableEntity));

        Assert.NotNull(entityType);
        Assert.True(entityType!.GetProperty(nameof(IAuditableEntity.Version)).IsConcurrencyToken);
        Assert.NotEmpty(entityType.GetDeclaredQueryFilters());
    }

    private static TestAuditableDbContext CreateDbContext(SqliteConnection connection)
    {
        return new TestAuditableDbContext(
            new DbContextOptionsBuilder<TestAuditableDbContext>()
                .UseSqlite(connection)
                .Options,
            new FakeAuditActorAccessor());
    }

    private sealed class TestAuditableDbContext : AuditableDbContext
    {
        public TestAuditableDbContext(
            DbContextOptions<TestAuditableDbContext> options,
            IAuditActorAccessor auditActorAccessor)
            : base(options, auditActorAccessor)
        {
        }

        public DbSet<TestAuditableEntity> Entities => Set<TestAuditableEntity>();
    }

    private sealed class FakeAuditActorAccessor : IAuditActorAccessor
    {
        public string AuditActor => "actor-001";
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public const string Actor = "current-user-actor";

        public Guid? UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");

        public string AuditActor => Actor;
    }

    private sealed class TestAuditableEntity : AuditableEntity<Guid>
    {
        private TestAuditableEntity()
        {
        }

        public string Name { get; private set; } = string.Empty;

        public static TestAuditableEntity Create(string name)
        {
            return new TestAuditableEntity
            {
                Id = Guid.NewGuid(),
                Name = name
            };
        }

        public void Rename(string name)
        {
            Name = name;
        }
    }
}
