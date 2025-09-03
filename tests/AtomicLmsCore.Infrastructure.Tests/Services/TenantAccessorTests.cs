using AtomicLmsCore.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AtomicLmsCore.Infrastructure.Tests.Services;

public class TenantAccessorTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly TenantAccessor _tenantAccessor;

    public TenantAccessorTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContext = new Mock<HttpContext>();
        _tenantAccessor = new TenantAccessor(_mockHttpContextAccessor.Object);
    }

    [Fact]
    public void GetCurrentTenantId_ValidTenantIdInContext_ReturnsTenantId()
    {
        // Arrange
        var expectedTenantId = Guid.NewGuid();
        var items = new Dictionary<object, object?>
        {
            { "ValidatedTenantId", expectedTenantId }
        };

        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act
        var result = _tenantAccessor.GetCurrentTenantId();

        // Assert
        result.Should().Be(expectedTenantId);
    }

    [Fact]
    public void GetCurrentTenantId_NoHttpContext_ReturnsNull()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(null as HttpContext);

        // Act
        var result = _tenantAccessor.GetCurrentTenantId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentTenantId_NoTenantIdInItems_ReturnsNull()
    {
        // Arrange
        var items = new Dictionary<object, object?>();
        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act
        var result = _tenantAccessor.GetCurrentTenantId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentTenantId_TenantIdIsNotGuid_ReturnsNull()
    {
        // Arrange
        var items = new Dictionary<object, object?>
        {
            { "ValidatedTenantId", "not-a-guid" }
        };

        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act
        var result = _tenantAccessor.GetCurrentTenantId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentTenantId_TenantIdIsNull_ReturnsNull()
    {
        // Arrange
        var items = new Dictionary<object, object?>
        {
            { "ValidatedTenantId", null }
        };

        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act
        var result = _tenantAccessor.GetCurrentTenantId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCurrentTenantId_WrongKeyInItems_ReturnsNull()
    {
        // Arrange
        var items = new Dictionary<object, object?>
        {
            { "SomeOtherKey", Guid.NewGuid() }
        };

        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act
        var result = _tenantAccessor.GetCurrentTenantId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetRequiredCurrentTenantId_ValidTenantIdInContext_ReturnsTenantId()
    {
        // Arrange
        var expectedTenantId = Guid.NewGuid();
        var items = new Dictionary<object, object?>
        {
            { "ValidatedTenantId", expectedTenantId }
        };

        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act
        var result = _tenantAccessor.GetRequiredCurrentTenantId();

        // Assert
        result.Should().Be(expectedTenantId);
    }

    [Fact]
    public void GetRequiredCurrentTenantId_NoTenantIdInContext_ThrowsInvalidOperationException()
    {
        // Arrange
        var items = new Dictionary<object, object?>();
        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act & Assert
        var action = () => _tenantAccessor.GetRequiredCurrentTenantId();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("No valid tenant ID found. Ensure TenantValidationMiddleware is properly configured.");
    }

    [Fact]
    public void GetRequiredCurrentTenantId_NoHttpContext_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(null as HttpContext);

        // Act & Assert
        var action = () => _tenantAccessor.GetRequiredCurrentTenantId();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("No valid tenant ID found. Ensure TenantValidationMiddleware is properly configured.");
    }

    [Fact]
    public void GetRequiredCurrentTenantId_TenantIdIsNotGuid_ThrowsInvalidOperationException()
    {
        // Arrange
        var items = new Dictionary<object, object?>
        {
            { "ValidatedTenantId", "not-a-guid" }
        };

        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act & Assert
        var action = () => _tenantAccessor.GetRequiredCurrentTenantId();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("No valid tenant ID found. Ensure TenantValidationMiddleware is properly configured.");
    }

    [Theory]
    [InlineData(123)]
    [InlineData(true)]
    [InlineData("string")]
    [InlineData(null)]
    public void GetCurrentTenantId_NonGuidTypes_ReturnsNull(object? tenantIdValue)
    {
        // Arrange
        var items = new Dictionary<object, object?>
        {
            { "ValidatedTenantId", tenantIdValue }
        };

        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act
        var result = _tenantAccessor.GetCurrentTenantId();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(123)]
    [InlineData(true)]
    [InlineData("string")]
    [InlineData(null)]
    public void GetRequiredCurrentTenantId_NonGuidTypes_ThrowsInvalidOperationException(object? tenantIdValue)
    {
        // Arrange
        var items = new Dictionary<object, object?>
        {
            { "ValidatedTenantId", tenantIdValue }
        };

        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act & Assert
        var action = () => _tenantAccessor.GetRequiredCurrentTenantId();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("No valid tenant ID found. Ensure TenantValidationMiddleware is properly configured.");
    }

    [Fact]
    public void GetCurrentTenantId_EmptyGuid_ReturnsEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var items = new Dictionary<object, object?>
        {
            { "ValidatedTenantId", emptyGuid }
        };

        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act
        var result = _tenantAccessor.GetCurrentTenantId();

        // Assert
        result.Should().Be(emptyGuid);
    }

    [Fact]
    public void GetRequiredCurrentTenantId_EmptyGuid_ReturnsEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var items = new Dictionary<object, object?>
        {
            { "ValidatedTenantId", emptyGuid }
        };

        _mockHttpContext.Setup(x => x.Items).Returns(items);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act
        var result = _tenantAccessor.GetRequiredCurrentTenantId();

        // Assert
        result.Should().Be(emptyGuid);
    }
}
