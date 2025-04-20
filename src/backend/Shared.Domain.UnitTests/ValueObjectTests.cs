using System.Collections.Generic;
using Shouldly;

namespace Conduit.Shared.Domain.UnitTests;

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

    private class TestValueObject : ValueObject
    {
        public int Number { get; }
        public string Text { get; }

        public TestValueObject(int number, string text)
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
}
