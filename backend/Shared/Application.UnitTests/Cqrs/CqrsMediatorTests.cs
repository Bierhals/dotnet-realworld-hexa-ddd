using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;
using Moq;
using Shouldly;

namespace Conduit.Shared.Application.UnitTests.Cqrs;

public class CqrsMediatorTests
{
    private readonly Mock<IServiceProvider> _spMock = new(MockBehavior.Strict);
    private readonly CqrsMediator _sut;
 
    public CqrsMediatorTests()
    {
        _sut = new CqrsMediator(_spMock.Object);
    }
 
    [Fact]
    public async Task Send_Command_ResolvesCorrectHandlerType_And_ReturnsSuccess()
    {
        // Arrange
        var command = new TestCommand { Payload = "hello" };
        using var cts = new CancellationTokenSource();
 
        var handlerMock = new Mock<ICommandHandler<TestCommand>>();
        handlerMock
            .Setup(h => h.Handle(command, cts.Token))
            .ReturnsAsync(Result.Success);

        SetupHandler(handlerMock.Object);
 
        // Act
        var result = await _sut.Send(command, cts.Token);
 
        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(Result.Success);
        handlerMock.Verify(h => h.Handle(command, cts.Token), Times.Once);
    }
 
    [Fact]
    public async Task Send_Command_HandlerReturnsError_PropagatesError()
    {
        // Arrange
        var expectedError = Error.Validation("Test.Invalid", "Something went wrong");
        var command = new TestCommand();
 
        var handlerMock = new Mock<ICommandHandler<TestCommand>>();
        handlerMock
            .Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedError);

        SetupHandler(handlerMock.Object);
 
        // Act
        var result = await _sut.Send(command, CancellationToken.None);
 
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(expectedError);
    }
 
    [Fact]
    public async Task Send_Command_NoHandlerRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupMissingHandler<ICommandHandler<TestCommand>>();
 
        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.Send(new TestCommand(), CancellationToken.None));
    }
 
    [Fact]
    public async Task Send_CommandWithResponse_ResolvesCorrectHandlerType_And_ReturnsValue()
    {
        // Arrange
        var expectedValue = "processed:abc";
        var command = new TestCommandWithResponse { Payload = "abc" };
 
        var handlerMock = new Mock<ICommandHandler<TestCommandWithResponse, string>>();
        handlerMock
            .Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)expectedValue.Clone());

        SetupHandler(handlerMock.Object);
 
        // Act
        var result = await _sut.Send(command, CancellationToken.None);
 
        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe(expectedValue);
        handlerMock.Verify(h => h.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
    }
 
    [Fact]
    public async Task Send_CommandWithResponse_HandlerReturnsError_PropagatesError()
    {
        // Arrange
        var expectedError = Error.NotFound("Test.NotFound", "Nicht gefunden");
 
        var handlerMock = new Mock<ICommandHandler<TestCommandWithResponse, string>>();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestCommandWithResponse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedError);

        SetupHandler(handlerMock.Object);
 
        // Act
        var result = await _sut.Send(new TestCommandWithResponse(), CancellationToken.None);
 
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(expectedError);
    }
 
    [Fact]
    public async Task Send_CommandWithResponse_NoHandlerRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupMissingHandler<ICommandHandler<TestCommandWithResponse, string>>();
 
        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.Send(new TestCommandWithResponse(), CancellationToken.None));
    }
 
    [Fact]
    public async Task Send_Query_ResolvesCorrectHandlerType_And_ReturnsValue()
    {
        // Arrange
        var query = new TestQuery { Filter = "active" };
        using var cts = new CancellationTokenSource();
 
        var handlerMock = new Mock<IQueryHandler<TestQuery, string>>();
        handlerMock
            .Setup(h => h.Handle(query, cts.Token))
            .ReturnsAsync("result:active");

        SetupHandler(handlerMock.Object);
 
        // Act
        var result = await _sut.Send(query, cts.Token);
 
        // Assert
        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe("result:active");
        handlerMock.Verify(h => h.Handle(query, cts.Token), Times.Once);
    }
 
    [Fact]
    public async Task Send_Query_HandlerReturnsError_PropagatesError()
    {
        // Arrange
        var expectedError = Error.Unauthorized("Test.Unauthorized", "Kein Zugriff");
 
        var handlerMock = new Mock<IQueryHandler<TestQuery, string>>();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedError);

        SetupHandler(handlerMock.Object);
 
        // Act
        var result = await _sut.Send(new TestQuery(), CancellationToken.None);
 
        // Assert
        result.IsError.ShouldBeTrue();
        result.FirstError.ShouldBe(expectedError);
    }
 
    [Fact]
    public async Task Send_Query_NoHandlerRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        SetupMissingHandler<IQueryHandler<TestQuery, string>>();
 
        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(
            () => _sut.Send(new TestQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Send_Command_WithMultipleCommandHandlersRegistered_InvokesOnlyMatchingHandler()
    {
        // Arrange
        var matchingHandler = new Mock<ICommandHandler<TestCommand>>();
        matchingHandler
            .Setup(h => h.Handle(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
 
        var otherHandler = new Mock<ICommandHandler<TestCommandTwo>>();
        otherHandler
            .Setup(h => h.Handle(It.IsAny<TestCommandTwo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        SetupHandler(matchingHandler.Object);
        SetupHandler(otherHandler.Object);
 
        // Act
        _ = await _sut.Send(new TestCommand(), CancellationToken.None);
 
        // Assert
        matchingHandler.Verify(h => h.Handle(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        otherHandler.Verify(h => h.Handle(It.IsAny<TestCommandTwo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Send_CommandWithResponse_WithMultipleCommandWithResponseHandlersRegistered_InvokesOnlyMatchingHandler()
    {
        // Arrange
        var matchingHandler = new Mock<ICommandHandler<TestCommandWithResponse, string>>();
        matchingHandler
            .Setup(h => h.Handle(It.IsAny<TestCommandWithResponse>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("matching-response");
 
        var otherHandler = new Mock<ICommandHandler<TestCommandWithResponseTwo, string>>();
        otherHandler
            .Setup(h => h.Handle(It.IsAny<TestCommandWithResponseTwo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("other-response");

        SetupHandler(matchingHandler.Object);
        SetupHandler(otherHandler.Object);
 
        // Act
        _ = await _sut.Send(new TestCommandWithResponse(), CancellationToken.None);
 
        // Assert
        matchingHandler.Verify(h => h.Handle(It.IsAny<TestCommandWithResponse>(), It.IsAny<CancellationToken>()), Times.Once);
        otherHandler.Verify(h => h.Handle(It.IsAny<TestCommandWithResponseTwo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Send_Query_WithMultipleQueryHandlersRegistered_InvokesOnlyMatchingHandler()
    {
        // Arrange
        var matchingHandler = new Mock<IQueryHandler<TestQuery, string>>();
        matchingHandler
            .Setup(h => h.Handle(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("matching-response");
 
        var otherHandler = new Mock<IQueryHandler<TestQueryTwo, string>>();
        otherHandler
            .Setup(h => h.Handle(It.IsAny<TestQueryTwo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("other-response");

        SetupHandler(matchingHandler.Object);
        SetupHandler(otherHandler.Object);
 
        // Act
        _ = await _sut.Send(new TestQuery(), CancellationToken.None);
 
        // Assert
        matchingHandler.Verify(h => h.Handle(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        otherHandler.Verify(h => h.Handle(It.IsAny<TestQueryTwo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupHandler<THandler>(THandler handlerInstance) where THandler : class
        => _spMock
            .Setup(x => x.GetService(typeof(THandler)))
            .Returns(handlerInstance);
 
    private void SetupMissingHandler<THandler>() where THandler : class
        => _spMock
            .Setup(x => x.GetService(typeof(THandler)))
            .Returns((object?)null);

    public class TestCommand : ICommand
    {
        public string Payload { get; init; } = string.Empty;
    }

    public class TestCommandTwo : ICommand
    {
        public string Payload { get; init; } = string.Empty;
    }

 
    public class TestCommandWithResponse : ICommand<string>
    {
        public string Payload { get; init; } = string.Empty;
    }

    public class TestCommandWithResponseTwo : ICommand<string>
    {
        public string Payload { get; init; } = string.Empty;
    }
 
    public class TestQuery : IQuery<string>
    {
        public string Filter { get; init; } = string.Empty;
    }

    public class TestQueryTwo : IQuery<string>
    {
        public string Filter { get; init; } = string.Empty;
    }
}