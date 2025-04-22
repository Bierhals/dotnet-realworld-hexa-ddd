using Conduit.Shared.Domain.BuildingBlocks;
using ErrorOr;
using Shouldly;

namespace Conduit.Shared.Domain.UnitTests.BuildingBlocks;

public class EntityTests
{
    [Fact]
    public void Entities_WithSameId_ShouldBeEqual()
    {
        var entity1 = new TestEntity(1, "Entity");
        var entity2 = new TestEntity(1, "Entity");

        entity1.ShouldBe(entity2);
        entity1.GetHashCode().ShouldBe(entity2.GetHashCode());
    }

    [Fact]
    public void Entities_WithoutIds_ShouldNotBeEqual()
    {
        var entity1 = new TestEntity("Entity");
        var entity2 = new TestEntity("Entity");

        entity1.ShouldNotBe(entity2);
    }

    [Fact]
    public void Entities_WithDifferentIds_ShouldNotBeEqual()
    {
        var entity1 = new TestEntity(1, "Entity A");
        var entity2 = new TestEntity(2, "Entity A");

        entity1.ShouldNotBe(entity2);
    }

    [Fact]
    public void Entities_WithSameId_ButDifferentProperties_ShouldStillBeEqual()
    {
        var entity1 = new TestEntity(1, "Name A");
        var entity2 = new TestEntity(1, "Name B");

        entity1.ShouldBe(entity2);
    }

    [Fact]
    public void Null_Entity_ShouldNotBeEqualTo_NonNull_Entity()
    {
        var entity = new TestEntity(1, "Entity A");

        entity.ShouldNotBe(null);
    }

    [Fact]
    public void SameEntityInstance_ShouldBeEqualToItself()
    {
        var entity = new TestEntity(1, "Entity A");

        entity.ShouldBe(entity);
    }

    [Fact]
    public void Entity_OperatorEquality_ShouldReturnTrue_ForSameId()
    {
        var entity1 = new TestEntity(1, "Entity A");
        var entity2 = new TestEntity(1, "Entity B");

        (entity1 == entity2).ShouldBeTrue();
        (entity1 != entity2).ShouldBeFalse();
    }

    [Fact]
    public void Entity_OperatorInequality_ShouldReturnTrue_ForDifferentIds()
    {
        var entity1 = new TestEntity(1, "Entity A");
        var entity2 = new TestEntity(2, "Entity A");

        (entity1 == entity2).ShouldBeFalse();
        (entity1 != entity2).ShouldBeTrue();
    }

    [Fact]
    public void Entities_OfDifferentTypes_ShouldNotBeEqual()
    {
        var entity1 = new TestEntity(1, "Entity A");
        var entity2 = new AnotherTestEntity(1, "Entity A");

        entity1.ShouldNotBe<Entity<int>>(entity2);
    }

    [Fact]
    public void Entity_WithValidRules_ReturnsSuccess()
    {
        var entity = new TestEntity(1, "Entity A");
        var rules = new IBusinessRule[]
        {
            new AlwaysValidRule(),
            new AlwaysValidRule()
        };

        var result = entity.CheckRules(rules);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeOfType<Success>();
    }

    [Fact]
    public void Entity_WithInvalidRule_ReturnsError()
    {
        var entity = new TestEntity(1, "Entity A");
        var rules = new IBusinessRule[]
        {
            new AlwaysInvalidRule("SomeRule", "Some rule failed")
        };

        var result = entity.CheckRules(rules);

        result.IsError.ShouldBeTrue();
        var errors = result.Errors;
        errors.Count.ShouldBe(1);
        errors[0].Code.ShouldBe("SomeRule");
        errors[0].Description.ShouldBe("Some rule failed");
    }

    private class TestEntity : Entity<int>
    {
        public string Name { get; }

        public TestEntity(int id, string name) : base(id)
        {
            Name = name;
        }

        public TestEntity(string name) : base()
        {
            Name = name;
        }

        public ErrorOr<Success> CheckRules(params IBusinessRule[] rules)
        {
            return Check(rules);
        }
    }

    private class AnotherTestEntity : Entity<int>
    {
        public string Name { get; }

        public AnotherTestEntity(int id, string name) : base(id)
        {
            Name = name;
        }

        public AnotherTestEntity(string name) : base()
        {
            Name = name;
        }
    }

    private class AlwaysValidRule : IBusinessRule
    {
        public ErrorOr<Success> Check() => Result.Success;
    }

    private class AlwaysInvalidRule : IBusinessRule
    {
        private readonly Error _error;

        public AlwaysInvalidRule(string code = "RuleViolated", string description = "Rule violated")
        {
            _error = Error.Validation(code, description);
        }

        public ErrorOr<Success> Check() => _error;
    }
}
