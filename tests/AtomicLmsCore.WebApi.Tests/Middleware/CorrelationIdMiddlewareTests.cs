using AtomicLmsCore.WebApi.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.WebApi.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly CorrelationIdMiddleware _middleware;

    public CorrelationIdMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        var loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        _middleware = new CorrelationIdMiddleware(_nextMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WithExistingCorrelationIdHeader_UsesProvidedId()
    {
        // Arrange
        const string ExistingCorrelationId = "existing-correlation-id";
        var context = CreateHttpContext();
        context.Request.Headers["X-Correlation-ID"] = ExistingCorrelationId;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().Be(ExistingCorrelationId);
        context.Response.Headers["X-Correlation-ID"].Should().Contain(ExistingCorrelationId);
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithoutCorrelationIdHeader_GeneratesNewId()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"].Should().BeOfType<string>().Subject;
        correlationId.Should().NotBeNullOrEmpty();
        correlationId.Length.Should().Be(32); // GUID without hyphens

        context.Response.Headers["X-Correlation-ID"].Should().Contain(correlationId);
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyCorrelationIdHeader_GeneratesNewId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["X-Correlation-ID"] = string.Empty;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"].Should().BeOfType<string>().Subject;
        correlationId.Should().NotBeNullOrEmpty();
        correlationId.Length.Should().Be(32);

        context.Response.Headers["X-Correlation-ID"].Should().Contain(correlationId);
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespaceCorrelationIdHeader_UsesProvidedValue()
    {
        // Arrange
        const string WhitespaceId = "   ";
        var context = CreateHttpContext();
        context.Request.Headers["X-Correlation-ID"] = WhitespaceId;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        // The middleware actually uses the whitespace value as-is since it only checks IsNullOrEmpty
        context.Items["CorrelationId"].Should().Be(WhitespaceId);
        context.Response.Headers["X-Correlation-ID"].Should().Contain(WhitespaceId);
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_GeneratesValidGuidFormat()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"].Should().BeOfType<string>().Subject;

        // Should be able to parse as GUID
        var isValidGuid = Guid.TryParseExact(correlationId, "N", out _);
        isValidGuid.Should().BeTrue("Generated correlation ID should be a valid GUID in 'N' format");

        correlationId.Should().MatchRegex("^[0-9a-f]{32}$", "Should be 32 lowercase hex characters");
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_SetsResponseHeaderBeforeCallingNext()
    {
        // Arrange
        var context = CreateHttpContext();
        var nextCalled = false;

        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .Callback<HttpContext>(ctx =>
            {
                // Verify header is set when next middleware runs
                ctx.Response.Headers.Should().ContainKey("X-Correlation-ID");
                ctx.Items.Should().ContainKey("CorrelationId");
                nextCalled = true;
            });

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Theory]
    [InlineData("custom-correlation-123")]
    [InlineData("12345678-1234-1234-1234-123456789012")]
    [InlineData("UPPERCASE-CORRELATION")]
    [InlineData("with-special-chars!@#")]
    public async Task InvokeAsync_PreservesCustomCorrelationIdFormats(string customId)
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["X-Correlation-ID"] = customId;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Items["CorrelationId"].Should().Be(customId);
        context.Response.Headers["X-Correlation-ID"].Should().Contain(customId);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };
        return context;
    }
}
