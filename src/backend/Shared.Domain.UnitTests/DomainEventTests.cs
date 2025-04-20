using System;
using Shouldly;

namespace Conduit.Shared.Domain.UnitTests;

public class DomainEventTests
{
    [Fact]
    public void Constructor_Should_Initialize_Id()
    {
        var domainEvent = new TestDomainEvent();

        domainEvent.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_Should_Set_OccurredOn_To_CurrentTime()
    {
        var domainEvent = new TestDomainEvent();
        var now = DateTime.Now;

        domainEvent.OccurredOn.ShouldBeInRange(now.AddSeconds(-1), now);
    }

    [Fact]
    public void Two_Instances_Should_Not_Be_Equal()
    {
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        var isEqual = event1.Equals(event2);

        isEqual.ShouldBeFalse();
    }

    private class TestDomainEvent : DomainEvent { }
}
