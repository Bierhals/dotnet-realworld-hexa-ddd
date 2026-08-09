using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.EventHandling;
using Conduit.Shared.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Conduit.Shared.Application.UnitTests.EventHandling;

public sealed class LoggingDomainEventHandlerTests
{
    private readonly Mock<ILogger<LoggingDomainEventHandler<TestDomainEvent>>> _loggerMock = new();
    private readonly LoggingDomainEventHandler<TestDomainEvent> _sut;

    public LoggingDomainEventHandlerTests()
    {
        _loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
        _sut = new LoggingDomainEventHandler<TestDomainEvent>(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_AnyDomainEvent_LogsAtInformationLevel()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();

        // Act
        await _sut.Handle(domainEvent, CancellationToken.None);

        // Assert
        // CA1873 doesn't apply here: this Verify expression asserts on ILogger.Log itself, not a
        // logging call site guarded by IsEnabled.
#pragma warning disable CA1873
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
#pragma warning restore CA1873
    }

    [Fact]
    public async Task Handle_AnyDomainEvent_DoesNotThrow()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();

        // Act
        async Task act() => await _sut.Handle(domainEvent, CancellationToken.None);

        // Assert
        await Should.NotThrowAsync(act);
    }

    public record TestDomainEvent : DomainEvent;
}
