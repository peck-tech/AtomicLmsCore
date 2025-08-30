using AtomicLmsCore.Application.HelloWorld.Queries;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Application.Tests.HelloWorld.Queries;

public class GetHelloWorldQueryHandlerTests
{
    private readonly GetHelloWorldQueryHandler _handler;
    private readonly Mock<ILogger<GetHelloWorldQueryHandler>> _loggerMock;

    public GetHelloWorldQueryHandlerTests()
    {
        _loggerMock = new();
        _handler = new(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ReturnsSuccessWithGreeting()
    {
        var query = new GetHelloWorldQuery { Name = "John" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Hello, John!");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WithNullName_ReturnsSuccessWithDefaultGreeting()
    {
        var query = new GetHelloWorldQuery { Name = null };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Hello, World!");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnsSuccessWithDefaultGreeting()
    {
        var query = new GetHelloWorldQuery { Name = "" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Hello, World!");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WithWhitespaceName_ReturnsSuccessWithDefaultGreeting()
    {
        var query = new GetHelloWorldQuery { Name = "   " };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Hello, World!");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WithSpecialCharacters_ReturnsSuccessWithGreeting()
    {
        var query = new GetHelloWorldQuery { Name = "John@123!" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Hello, John@123!!");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        var query = new GetHelloWorldQuery { Name = "John" };
        var cancellationToken = new CancellationToken(true);

        Func<Task> act = async () => await _handler.Handle(query, cancellationToken);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_LogsInformation()
    {
        var query = new GetHelloWorldQuery { Name = "John" };

        await _handler.Handle(query, CancellationToken.None);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating greeting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
