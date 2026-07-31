
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.EventHandling;
using Conduit.Shared.Domain;
using Moq;
using Shouldly;

namespace Conduit.Shared.Application.UnitTests.Cqrs;

public sealed class DomainEventDispatcherTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly DomainEventDispatcher _sut;

    public DomainEventDispatcherTests()
    {
        _sut = new DomainEventDispatcher(_serviceProviderMock.Object);
    }

    [Fact]
    public async Task DispatchAsync_SingleEventWithSingleHandler_InvokesHandlerOnce()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();
        var handlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();

        SetupHandlers(handlerMock.Object);

        // Act
        await _sut.DispatchAsync([domainEvent], CancellationToken.None);

        // Assert
        handlerMock.Verify(
            h => h.Handle(domainEvent, CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_SingleEventWithMultipleHandlers_InvokesAllHandlers()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();
        var firstHandlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var secondHandlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();

        SetupHandlers(firstHandlerMock.Object, secondHandlerMock.Object);

        // Act
        await _sut.DispatchAsync([domainEvent], CancellationToken.None);

        // Assert
        firstHandlerMock.Verify(h => h.Handle(domainEvent, CancellationToken.None), Times.Once);
        secondHandlerMock.Verify(h => h.Handle(domainEvent, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_MultipleEventsOfDifferentTypes_ResolvesHandlersPerEventType()
    {
        // Arrange
        var testEvent = new TestDomainEvent();
        var otherEvent = new OtherTestDomainEvent();

        var testHandlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var otherHandlerMock = new Mock<IDomainEventHandler<OtherTestDomainEvent>>();

        SetupHandlers(testHandlerMock.Object);
        SetupHandlers(otherHandlerMock.Object);

        // Act
        await _sut.DispatchAsync([testEvent, otherEvent], CancellationToken.None);

        // Assert
        testHandlerMock.Verify(h => h.Handle(testEvent, CancellationToken.None), Times.Once);
        otherHandlerMock.Verify(h => h.Handle(otherEvent, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_NoHandlersRegisteredForEvent_DoesNotThrow()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();
        SetupHandlers<TestDomainEvent>(); // leere Handler-Liste

        // Act
        async Task act() => await _sut.DispatchAsync([domainEvent], CancellationToken.None);

        // Assert
        await Should.NotThrowAsync(act);
    }

    [Fact]
    public async Task DispatchAsync_EmptyEventCollection_DoesNotResolveAnyHandlers()
    {
        // Act
        await _sut.DispatchAsync([], CancellationToken.None);

        // Assert
        _serviceProviderMock.Verify(
            sp => sp.GetService(It.IsAny<Type>()),
            Times.Never);
    }

    [Fact]
    public async Task DispatchAsync_PassesProvidedCancellationTokenToHandler()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var domainEvent = new TestDomainEvent();
        var handlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();

        SetupHandlers(handlerMock.Object);

        // Act
        await _sut.DispatchAsync([domainEvent], cts.Token);

        // Assert
        handlerMock.Verify(h => h.Handle(domainEvent, cts.Token), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_HandlerThrows_ExceptionPropagatesAndStopsProcessing()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();
        var expectedException = new InvalidOperationException("Handler failed");

        var handlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        handlerMock
            .Setup(h => h.Handle(domainEvent, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        SetupHandlers(handlerMock.Object);

        // Act
        async Task act() => await _sut.DispatchAsync([domainEvent], CancellationToken.None);

        // Assert
        var thrown = await Should.ThrowAsync<InvalidOperationException>(act);
        thrown.ShouldBe(expectedException);
    }

    [Fact]
    public async Task DispatchAsync_SameEventTypeMultipleTimes_ResolvesHandlersForEachOccurrence()
    {
        // Arrange
        var firstEvent = new TestDomainEvent();
        var secondEvent = new TestDomainEvent();
        var handlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();

        SetupHandlers(handlerMock.Object);

        // Act
        await _sut.DispatchAsync([firstEvent, secondEvent], CancellationToken.None);

        // Assert
        handlerMock.Verify(h => h.Handle(firstEvent, CancellationToken.None), Times.Once);
        handlerMock.Verify(h => h.Handle(secondEvent, CancellationToken.None), Times.Once);
        handlerMock.Verify(h => h.Handle(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private void SetupHandlers<TEvent>(params IDomainEventHandler<TEvent>[] handlers)
        where TEvent : IDomainEvent
    {
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEnumerable<IDomainEventHandler<TEvent>>)))
            .Returns(handlers.AsEnumerable());
    }

    public record TestDomainEvent : DomainEvent;

    public record OtherTestDomainEvent : DomainEvent;

}
