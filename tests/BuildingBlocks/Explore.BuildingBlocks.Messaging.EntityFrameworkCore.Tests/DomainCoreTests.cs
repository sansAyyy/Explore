using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Exceptions;

namespace Explore.BuildingBlocks.Messaging.EntityFrameworkCore.Tests;

public sealed class DomainCoreTests
{
    [Fact]
    public void ValueObject_ShouldUseAtomicValues_ForEqualityAndHashCode()
    {
        var left = new TestValueObject("alpha", 1);
        var right = new TestValueObject("alpha", 1);
        var different = new TestValueObject("beta", 2);

        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
        Assert.NotEqual(left, different);
    }

    [Fact]
    public void Entity_ShouldAllowDerivedType_ToSetAndExposeId()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void AuditableEntity_ShouldExposeAllAuditingFields()
    {
        var entity = TestAuditableEntity.Create(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "creator",
            DateTime.UtcNow.AddMinutes(1),
            "updater",
            isDeleted: true,
            version: 3);

        Assert.Equal("creator", entity.CreatedBy);
        Assert.Equal("updater", entity.UpdatedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(3, entity.Version);
    }

    [Fact]
    public void DomainException_ShouldPreserveMessage()
    {
        var exception = new DomainException("domain failure");

        Assert.Equal("domain failure", exception.Message);
    }

    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id)
        {
            Id = id;
        }
    }

    private sealed class TestAuditableEntity : AuditableEntity<Guid>
    {
        private TestAuditableEntity()
        {
        }

        public static TestAuditableEntity Create(
            Guid id,
            DateTime createdAt,
            string createdBy,
            DateTime? updatedAt,
            string? updatedBy,
            bool isDeleted,
            long version)
        {
            return new TestAuditableEntity
            {
                Id = id,
                CreatedAt = createdAt,
                CreatedBy = createdBy,
                UpdatedAt = updatedAt,
                UpdatedBy = updatedBy,
                IsDeleted = isDeleted,
                Version = version
            };
        }
    }

    private sealed class TestValueObject : ValueObject
    {
        private readonly string _name;
        private readonly int _count;

        public TestValueObject(string name, int count)
        {
            _name = name;
            _count = count;
        }

        protected override IEnumerable<object?> GetAtomicValues()
        {
            yield return _name;
            yield return _count;
        }
    }
}
