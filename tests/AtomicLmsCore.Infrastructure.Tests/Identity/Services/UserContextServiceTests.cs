using System.Security.Claims;
using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Infrastructure.Identity.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Infrastructure.Tests.Identity.Services;

public class UserContextServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly UserContextService _sut;

    public UserContextServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var loggerMock = new Mock<ILogger<UserContextService>>();
        _sut = new UserContextService(_httpContextAccessorMock.Object, loggerMock.Object);
    }

    [Fact]
    public void AuthenticationType_WhenNotAuthenticated_ReturnsNone()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal()
        };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.AuthenticationType;

        // Assert
        result.Should().Be(AuthenticationType.None);
    }

    [Fact]
    public void AuthenticationType_WhenUserAuthenticated_ReturnsUser()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user123"),
            new Claim("sub", "user123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.AuthenticationType;

        // Assert
        result.Should().Be(AuthenticationType.User);
    }

    [Fact]
    public void AuthenticationType_WhenMachineAuthenticated_ReturnsMachine()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim("gty", "client-credentials"),
            new Claim("azp", "machine123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.AuthenticationType;

        // Assert
        result.Should().Be(AuthenticationType.Machine);
    }

    [Fact]
    public void AuthenticatedSubject_ForUserAuth_ReturnsUserId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.AuthenticatedSubject;

        // Assert
        result.Should().Be("user123");
    }

    [Fact]
    public void AuthenticatedSubject_ForMachineAuth_ReturnsClientId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim("gty", "client-credentials"),
            new Claim("azp", "machine123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.AuthenticatedSubject;

        // Assert
        result.Should().Be("machine123");
    }

    [Fact]
    public void TargetUserId_ForUserAuth_ReturnsAuthenticatedUserId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.TargetUserId;

        // Assert
        result.Should().Be("user123");
    }

    [Fact]
    public void TargetUserId_ForMachineAuth_ReturnsOnBehalfOfHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim("gty", "client-credentials"),
            new Claim("azp", "machine123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        context.Request.Headers["X-On-Behalf-Of"] = "user456";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.TargetUserId;

        // Assert
        result.Should().Be("user456");
    }

    [Fact]
    public void TargetUserId_ForMachineAuthWithoutHeader_ReturnsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim("gty", "client-credentials"),
            new Claim("azp", "machine123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.TargetUserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IsMachineToMachine_WhenMachineAuth_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim("gty", "client-credentials"),
            new Claim("azp", "machine123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.IsMachineToMachine;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMachineToMachine_WhenUserAuth_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.IsMachineToMachine;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateUserContext_WhenNotAuthenticated_ReturnsFailure()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.ValidateUserContext();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message == "Request is not authenticated");
    }

    [Fact]
    public void ValidateUserContext_ForUserAuth_ReturnsSuccess()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.ValidateUserContext();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateUserContext_ForMachineAuthWithHeader_ReturnsSuccess()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim("gty", "client-credentials"),
            new Claim("azp", "machine123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        context.Request.Headers["X-On-Behalf-Of"] = "user456";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.ValidateUserContext();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateUserContext_ForMachineAuthWithoutHeader_ReturnsFailure()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim("gty", "client-credentials"),
            new Claim("azp", "machine123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.ValidateUserContext();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("X-On-Behalf-Of header"));
    }

    [Fact]
    public void AuthenticationType_DetectsAuth0MachineClient()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "machine123@clients"),
            new Claim("sub", "machine123@clients")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = _sut.AuthenticationType;

        // Assert
        result.Should().Be(AuthenticationType.Machine);
    }

    [Fact]
    public void AuthenticationType_CachesResultInHttpContext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user123")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result1 = _sut.AuthenticationType;
        var result2 = _sut.AuthenticationType;

        // Assert
        result1.Should().Be(AuthenticationType.User);
        result2.Should().Be(AuthenticationType.User);
        context.Items.Should().ContainKey("AuthenticationType");
        context.Items["AuthenticationType"].Should().Be(AuthenticationType.User);
    }
}
