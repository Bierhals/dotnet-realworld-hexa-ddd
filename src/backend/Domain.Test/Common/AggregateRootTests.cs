using System;
using System.Collections.Generic;
using Conduit.Domain.Common;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Xunit;

namespace Conduit.Domain.Test.Common;

public class AggregateRootTests
{
    [Fact]
    public void DomainEvents_are_empty_after_clear()
    {
        MyAggregate aggregate = new(new MyId(1));
        aggregate.ActionWithEvent();

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void New_events_are_added_to_domainEvents()
    {
        MyAggregate aggregate = new(new MyId(1));

        aggregate.ActionWithEvent();

        aggregate.DomainEvents.Should().HaveCount(1);
    }

    public class MyAggregate : AggregateRoot<MyId>
    {
        public MyAggregate(MyId id) : base(id)
        {
        }

        public void ActionWithEvent()
        {
            AddDomainEvent(new TestEvent());
        }
    }

    public class MyId : ComparableValueObject
    {
        public int Value
        {
            get;
        }

        public MyId(int value)
        {
            Value = value;
        }

        protected override IEnumerable<IComparable> GetComparableEqualityComponents()
        {
            yield return Value;
        }
    }

    public class TestEvent : IDomainEvent
    {
        public Guid Id
        {
            get;
        }
        public DateTime OccurredOn
        {
            get;
        }

        public TestEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.Now;
        }
    }
}
