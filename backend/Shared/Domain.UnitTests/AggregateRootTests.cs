using System;
using System.Collections.Generic;
using Shouldly;

namespace Conduit.Shared.Domain.UnitTests;

public class AggregateRootTests
{
    [Fact]
    public void Constructor_Should_Set_Id_Correctly()
    {
        var expectedId = Guid.NewGuid();

        var aggregate = new TestAggregateRoot(Guid.Parse(expectedId.ToString()));

        aggregate.Id.ShouldBe(expectedId);
    }

    [Fact]
    public void AddDomainEvent_Should_Add_Event_To_List()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        var domainEvent = new TestEvent();

        aggregate.RaiseEvent(domainEvent);

        aggregate.DomainEvents.ShouldContain(domainEvent);
        aggregate.DomainEvents.Count.ShouldBe(1);
    }

    [Fact]
    public void ClearDomainEvents_Should_Remove_All_Events()
    {
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.RaiseEvent(new TestEvent());
        aggregate.RaiseEvent(new TestEvent());

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.ShouldBeEmpty();
    }

    private record TestEvent : DomainEvent { }

    private class TestAggregateRoot : AggregateRoot<Guid>
    {
        public TestAggregateRoot(Guid id) : base(id) { }

        public void RaiseEvent(IDomainEvent domainEvent) => RaiseDomainEvent(domainEvent);
    }


}
