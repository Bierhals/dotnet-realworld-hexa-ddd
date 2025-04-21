using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Conduit.Shared.Domain.BuildingBlocks;
using Moq;
using Shouldly;

namespace Conduit.Shared.Domain.UnitTests.BuildingBlocks;

public class DomainEventProcessorTests
{
    [Fact]
    public void Register_Should_Add_Handler_To_Dictionary()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var eventProcessor = new DomainEventProcessor(serviceProviderMock.Object);
        var fieldInfo = typeof(DomainEventProcessor).GetField("_handlerTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var handlerTypes = fieldInfo!.GetValue(eventProcessor) as Dictionary<Type, List<Type>>;

        // Act
        eventProcessor.Register<TestEvent, TestEventHandler>();

        // Assert
        handlerTypes.ShouldNotBeNull();
        handlerTypes.ShouldContainKey(typeof(TestEvent));
        handlerTypes[typeof(TestEvent)].ShouldContain(typeof(TestEventHandler));
    }

    [Fact]
    public async Task ProcessEvents_Should_Invoke_Handler()
    {
        // Arrange
        var testHandler = new TestEventHandler();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(TestEventHandler))).Returns(testHandler);

        var eventProcessor = new DomainEventProcessor(serviceProviderMock.Object);
        eventProcessor.Register<TestEvent, TestEventHandler>();

        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        var testEvent = new TestEvent();
        aggregate.AddEvent();

        // Act
        await eventProcessor.ProcessEvents(aggregate);

        // Assert
        testHandler.WasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task ProcessEvents_Should_Clear_DomainEvents()
    {
        // Arrange
        var testHandler = new TestEventHandler();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(TestEventHandler))).Returns(testHandler);

        var eventProcessor = new DomainEventProcessor(serviceProviderMock.Object);
        eventProcessor.Register<TestEvent, TestEventHandler>();

        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.AddEvent();

        // Act
        await eventProcessor.ProcessEvents(aggregate);

        // Assert
        aggregate.DomainEvents.ShouldBeEmpty();
    }

    private class TestEvent : DomainEvent { }

    private class TestAggregateRoot : AggregateRoot<Guid>
    {
        public TestAggregateRoot(Guid id) : base(id) { }

        public void AddEvent()
        {
            AddDomainEvent(new TestEvent());
        }
    }
    private class TestEventHandler : IDomainEventHandler<TestEvent>
    {
        public bool WasCalled { get; private set; }

        public Task Handle(TestEvent domainEvent)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }
}
