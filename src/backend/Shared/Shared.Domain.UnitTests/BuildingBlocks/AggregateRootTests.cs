using System;
using System.Collections.Generic;
using Conduit.Shared.Domain.BuildingBlocks;
using Shouldly;

namespace Conduit.Shared.Domain.UnitTests.BuildingBlocks;

public class AggregateRootTests
{
    [Fact]
    public void Constructor_Should_Set_Id_Correctly()
    {
        var id = new TestId();

        var aggregate = new TestAggregateRoot(id);

        aggregate.Id.ShouldBe(id);
    }

    [Fact]
    public void EConstructor_Without_Id_ShouldBe_default()
    {
        var aggregate = new TestAggregateRoot();

        aggregate.Id.ShouldBe(default(TestId));
    }

    [Fact]
    public void AddDomainEvent_Should_Add_Event_To_List()
    {
        var aggregate = new TestAggregateRoot(new TestId());
        var domainEvent = new TestEvent();

        aggregate.RaiseEvent(domainEvent);

        aggregate.DomainEvents.ShouldContain(domainEvent);
        aggregate.DomainEvents.Count.ShouldBe(1);
    }

    [Fact]
    public void ClearDomainEvents_Should_Remove_All_Events()
    {
        var aggregate = new TestAggregateRoot(new TestId());
        aggregate.RaiseEvent(new TestEvent());
        aggregate.RaiseEvent(new TestEvent());

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.ShouldBeEmpty();
    }

    private class TestId : ValueObject
    {
        public Guid Value { get; } = Guid.NewGuid();

        public TestId()
        {
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    private class TestEvent : DomainEvent { }

    private class TestAggregateRoot : AggregateRoot<TestId>
    {
        public TestAggregateRoot() : base() { }
        public TestAggregateRoot(TestId id) : base(id) { }

        public void RaiseEvent(DomainEvent domainEvent) => AddDomainEvent(domainEvent);
    }


}
