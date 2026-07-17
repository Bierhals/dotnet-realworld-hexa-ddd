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
    public void Constructor_Should_Set_OccurredOnUtc_To_CurrentTime()
    {
        var domainEvent = new TestDomainEvent();
        var now = DateTime.UtcNow;

        domainEvent.OccurredOnUtc.ShouldBeInRange(now.AddSeconds(-1), now);
    }

    [Fact]
    public void Constructor_Should_Set_OccurredOnUtc_Kind_To_Utc()
    {
        var domainEvent = new TestDomainEvent();

        domainEvent.OccurredOnUtc.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void Two_Instances_Should_Not_Be_Equal()
    {
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        var isEqual = event1.Equals(event2);

        isEqual.ShouldBeFalse();
    }

    private record TestDomainEvent : DomainEvent { }
}
