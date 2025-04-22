using System.Collections.Generic;
using Conduit.Shared.Domain.BuildingBlocks;
using ErrorOr;
using Shouldly;

namespace Conduit.Shared.Domain.UnitTests.BuildingBlocks;

public class ValueObjectTests
{
    [Fact]
    public void ValueObjects_WithSameValues_ShouldBeEqual()
    {
        var obj1 = new TestValueObject(42, "Hello");
        var obj2 = new TestValueObject(42, "Hello");

        obj1.ShouldBe(obj2);
        obj1.GetHashCode().ShouldBe(obj2.GetHashCode());
    }

    [Fact]
    public void ValueObjects_WithDifferentValues_ShouldNotBeEqual()
    {
        var obj1 = new TestValueObject(42, "Hello");
        var obj2 = new TestValueObject(99, "World");

        obj1.ShouldNotBe(obj2);
    }

    [Fact]
    public void ValueObjects_OfDifferentTypes_ShouldNotBeEqual()
    {
        var obj1 = new TestValueObject(42, "Hello");
        var obj2 = new AnotherValueObject(42, "Hello");

        var isEqual = obj1.Equals(obj2);

        isEqual.ShouldBeFalse();
        //obj1.ShouldNotBe<ValueObject>(obj2);
    }

    [Fact]
    public void ValueObject_EqualsNull_ShouldReturnFalse()
    {
        var obj = new TestValueObject(42, "Hello");

        obj.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void ValueObject_OperatorEquality_ShouldReturnTrue_ForSameValues()
    {
        var obj1 = new TestValueObject(42, "Hello");
        var obj2 = new TestValueObject(42, "Hello");

        (obj1 == obj2).ShouldBeTrue();
        (obj1 != obj2).ShouldBeFalse();
    }

    [Fact]
    public void ValueObject_OperatorEquality_ShouldReturnFalse_ForDifferentValues()
    {
        var obj1 = new TestValueObject(42, "Hello");
        var obj2 = new TestValueObject(99, "World");

        (obj1 == obj2).ShouldBeFalse();
        (obj1 != obj2).ShouldBeTrue();
    }

    [Fact]
    public void ValueObject_WithValidRules_ReturnsCreatedObject()
    {
        var rules = new IBusinessRule[]
        {
            new AlwaysValidRule(),
            new AlwaysValidRule()
        };

        var result = TestValueObject.CreateWithRuleCheck(rules);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeOfType<TestValueObject>();
    }

    [Fact]
    public void ValueObject_WithInvalidRule_ReturnsError()
    {
        var rules = new IBusinessRule[]
        {
            new AlwaysInvalidRule("SomeRule", "Some rule failed")
        };

        var result = TestValueObject.CreateWithRuleCheck(rules);

        result.IsError.ShouldBeTrue();
        var errors = result.Errors;
        errors.Count.ShouldBe(1);
        errors[0].Code.ShouldBe("SomeRule");
        errors[0].Description.ShouldBe("Some rule failed");
    }

    private class TestValueObject : ValueObject
    {
        public int Number { get; }
        public string Text { get; }

        public TestValueObject(int number, string text)
        {
            Number = number;
            Text = text;
        }

        public static ErrorOr<TestValueObject> CreateWithRuleCheck(params IBusinessRule[] rules)
        {
            var result = Check(rules);

            if (result.IsError)
                return result.Errors;

            return new TestValueObject(42, "Hello");
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Number;
            yield return Text;
        }
    }

    private class AnotherValueObject : ValueObject
    {
        public int Number { get; }
        public string Text { get; }

        public AnotherValueObject(int number, string text)
        {
            Number = number;
            Text = text;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Number;
            yield return Text;
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
