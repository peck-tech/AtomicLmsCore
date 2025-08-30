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
        result.Value.Message.Should().Be("Hello John, welcome to AtomicLMS Core!");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WithNullName_ReturnsSuccessWithDefaultGreeting()
    {
        var query = new GetHelloWorldQuery { Name = null };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Hello World from AtomicLMS Core!");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnsSuccessWithDefaultGreeting()
    {
        var query = new GetHelloWorldQuery { Name = "" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Hello World from AtomicLMS Core!");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WithWhitespaceName_ReturnsSuccessWithDefaultGreeting()
    {
        var query = new GetHelloWorldQuery { Name = "   " };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Hello World from AtomicLMS Core!");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WithSpecialCharacters_ReturnsSuccessWithGreeting()
    {
        var query = new GetHelloWorldQuery { Name = "John@123!" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Message.Should().Be("Hello John@123!, welcome to AtomicLMS Core!");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Processing Hello World request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
