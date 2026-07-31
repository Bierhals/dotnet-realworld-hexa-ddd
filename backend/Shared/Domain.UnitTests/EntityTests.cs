using System;
using Shouldly;

namespace Conduit.Shared.Domain.UnitTests;

public class EntityTests
{
    [Fact]
    public void Constructor_Should_Set_Id_Correctly()
    {
        var expectedId = Guid.NewGuid();

        var entity = new TestEntity(Guid.Parse(expectedId.ToString()), "Entity A");

        entity.Id.ShouldBe(expectedId);
    }

    [Fact]
    public void Entities_WithSameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Entity A");
        var entity2 = new TestEntity(id, "Entity A");

        entity1.ShouldBe(entity2);
        entity1.GetHashCode().ShouldBe(entity2.GetHashCode());
    }

    [Fact]
    public void Entities_WithDifferentIds_ShouldNotBeEqual()
    {
        var entity1 = new TestEntity(Guid.NewGuid(), "Entity A");
        var entity2 = new TestEntity(Guid.NewGuid(), "Entity B");

        entity1.ShouldNotBe(entity2);
    }

    [Fact]
    public void Entities_WithSameId_ButDifferentProperties_ShouldStillBeEqual()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Entity A");
        var entity2 = new TestEntity(id, "Entity A2");

        entity1.ShouldBe(entity2);
    }

    [Fact]
    public void Null_Entity_ShouldNotBeEqualTo_NonNull_Entity()
    {
        var entity = new TestEntity(Guid.NewGuid(), "Entity A");

        entity.ShouldNotBe(null);
    }

    [Fact]
    public void SameEntityInstance_ShouldBeEqualToItself()
    {
        var entity = new TestEntity(Guid.NewGuid(), "Entity A");

        entity.ShouldBe(entity);
    }

    [Fact]
    public void Entity_OperatorEquality_ShouldReturnTrue_ForSameId()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Entity A");
        var entity2 = new TestEntity(id, "Entity A2");

        (entity1 == entity2).ShouldBeTrue();
        (entity1 != entity2).ShouldBeFalse();
    }

    [Fact]
    public void Entity_OperatorInequality_ShouldReturnTrue_ForDifferentIds()
    {
        var entity1 = new TestEntity(Guid.NewGuid(), "Entity A");
        var entity2 = new TestEntity(Guid.NewGuid(), "Entity A");

        (entity1 == entity2).ShouldBeFalse();
        (entity1 != entity2).ShouldBeTrue();
    }

    [Fact]
    public void Entities_OfDifferentTypes_ShouldNotBeEqual()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id, "Entity A");
        var entity2 = new AnotherTestEntity(id, "Entity A");

        entity1.ShouldNotBe<Entity<Guid>>(entity2);
    }

    private class TestEntity : Entity<Guid>
    {
        public string Name { get; }

        public TestEntity(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }

    private class AnotherTestEntity : Entity<Guid>
    {
        public string Name { get; }

        public AnotherTestEntity(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }
}
