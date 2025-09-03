using System.Security.Claims;
using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Infrastructure.Identity.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Infrastructure.Tests.Identity.Services;

public class PermissionServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IUserContextService> _userContextServiceMock;
    private readonly PermissionService _sut;

    public PermissionServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _userContextServiceMock = new Mock<IUserContextService>();
        var loggerMock = new Mock<ILogger<PermissionService>>();
        _sut = new PermissionService(_httpContextAccessorMock.Object, _userContextServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task HasPermissionAsync_WhenNotAuthenticated_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal()
        };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = await _sut.HasPermissionAsync(Permissions.Users.Read);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_WithUserRole_SuperAdmin_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "superadmin")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _userContextServiceMock.Setup(x => x.IsMachineToMachine).Returns(false);
        _userContextServiceMock.Setup(x => x.AuthenticatedSubject).Returns("user123");

        // Act
        var result = await _sut.HasPermissionAsync(Permissions.Tenants.Read);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_WithUserRole_Admin_HasUserPermissions()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "admin")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _userContextServiceMock.Setup(x => x.IsMachineToMachine).Returns(false);
        _userContextServiceMock.Setup(x => x.AuthenticatedSubject).Returns("user123");

        // Act
        var hasUserPermission = await _sut.HasPermissionAsync(Permissions.Users.Read);
        var hasTenantPermission = await _sut.HasPermissionAsync(Permissions.Tenants.Read);

        // Assert
        hasUserPermission.Should().BeTrue();
        hasTenantPermission.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_WithMachineScope_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim("scope", "users:read tenants:create")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _userContextServiceMock.Setup(x => x.IsMachineToMachine).Returns(true);
        _userContextServiceMock.Setup(x => x.AuthenticatedSubject).Returns("machine123");

        // Act
        var hasUserRead = await _sut.HasPermissionAsync(Permissions.Users.Read);
        var hasTenantCreate = await _sut.HasPermissionAsync(Permissions.Tenants.Create);
        var hasTenantRead = await _sut.HasPermissionAsync(Permissions.Tenants.Read);

        // Assert
        hasUserRead.Should().BeTrue();
        hasTenantCreate.Should().BeTrue();
        hasTenantRead.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_WithPermissionClaim_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim("permission", Permissions.Users.Update)
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _userContextServiceMock.Setup(x => x.IsMachineToMachine).Returns(false);
        _userContextServiceMock.Setup(x => x.AuthenticatedSubject).Returns("user123");

        // Act
        var result = await _sut.HasPermissionAsync(Permissions.Users.Update);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_WithHierarchicalPermission_ReturnsTrue()
    {
        // Arrange - User has 'users:manage' which should grant 'users:read'
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "admin") // admin role has users:manage
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _userContextServiceMock.Setup(x => x.IsMachineToMachine).Returns(false);
        _userContextServiceMock.Setup(x => x.AuthenticatedSubject).Returns("user123");

        // Act
        var hasManagePermission = await _sut.HasPermissionAsync(Permissions.Users.Manage);
        var hasReadPermission = await _sut.HasPermissionAsync(Permissions.Users.Read);
        var hasCreatePermission = await _sut.HasPermissionAsync(Permissions.Users.Create);

        // Assert
        hasManagePermission.Should().BeTrue();
        hasReadPermission.Should().BeTrue(); // Should be granted through hierarchy
        hasCreatePermission.Should().BeTrue(); // Should be granted through hierarchy
    }

    [Fact]
    public async Task HasAnyPermissionAsync_WithOneValidPermission_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "learner") // only has learning:read
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _userContextServiceMock.Setup(x => x.IsMachineToMachine).Returns(false);
        _userContextServiceMock.Setup(x => x.AuthenticatedSubject).Returns("user123");

        // Act
        var result = await _sut.HasAnyPermissionAsync(
            Permissions.Users.Create,
            Permissions.LearningObjects.Read,
            Permissions.Tenants.Delete);

        // Assert
        result.Should().BeTrue(); // Should have learning:read
    }

    [Fact]
    public async Task HasAllPermissionsAsync_WithAllValidPermissions_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "superadmin") // has all permissions
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _userContextServiceMock.Setup(x => x.IsMachineToMachine).Returns(false);
        _userContextServiceMock.Setup(x => x.AuthenticatedSubject).Returns("user123");

        // Act
        var result = await _sut.HasAllPermissionsAsync(
            Permissions.Users.Read,
            Permissions.Tenants.Read,
            Permissions.LearningObjects.Read);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAllPermissionsAsync_WithMissingPermission_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "learner") // only has learning:read
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _userContextServiceMock.Setup(x => x.IsMachineToMachine).Returns(false);
        _userContextServiceMock.Setup(x => x.AuthenticatedSubject).Returns("user123");

        // Act
        var result = await _sut.HasAllPermissionsAsync(
            Permissions.LearningObjects.Read,
            Permissions.Users.Read); // This permission is missing

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPermissionsAsync_WithMultipleRoles_ReturnsAllPermissions()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Role, "instructor"),
            new Claim("scope", "custom:permission"),
            new Claim("permission", "extra:permission")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var permissions = await _sut.GetPermissionsAsync();
        var permissionList = permissions.ToList();

        // Assert
        permissionList.Should().Contain(Permissions.Users.Manage); // from admin
        permissionList.Should().Contain(Permissions.LearningObjects.Create); // from instructor
        permissionList.Should().Contain("custom:permission"); // from scope
        permissionList.Should().Contain("extra:permission"); // from permission claim
    }

    [Fact]
    public async Task ValidatePermissionAsync_WithValidPermission_ReturnsSuccess()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "admin")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _userContextServiceMock.Setup(x => x.IsMachineToMachine).Returns(false);
        _userContextServiceMock.Setup(x => x.AuthenticatedSubject).Returns("user123");
        _userContextServiceMock.Setup(x => x.TargetUserId).Returns("user123");

        // Act
        var result = await _sut.ValidatePermissionAsync(Permissions.Users.Read);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePermissionAsync_WithInvalidPermission_ReturnsFailureWithMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "learner") // doesn't have users:delete
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _userContextServiceMock.Setup(x => x.IsMachineToMachine).Returns(false);
        _userContextServiceMock.Setup(x => x.AuthenticatedSubject).Returns("user123");
        _userContextServiceMock.Setup(x => x.TargetUserId).Returns("user123");

        // Act
        var result = await _sut.ValidatePermissionAsync(Permissions.Users.Delete);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("does not have permission"));
        result.Errors.First().Message.Should().Contain(Permissions.Users.Delete);
    }

    [Fact]
    public async Task ValidatePermissionAsync_WithMachineAuth_IncludesBothIdentitiesInMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim("scope", "limited:scope") // doesn't have users:delete
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);
        _userContextServiceMock.Setup(x => x.IsMachineToMachine).Returns(true);
        _userContextServiceMock.Setup(x => x.AuthenticatedSubject).Returns("machine123");
        _userContextServiceMock.Setup(x => x.TargetUserId).Returns("user456");

        // Act
        var result = await _sut.ValidatePermissionAsync(Permissions.Users.Delete);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.First().Message.Should().Contain("Machine 'machine123' acting on behalf of user 'user456'");
    }

    [Fact]
    public async Task HasPermissionAsync_WithEmptyPermissionString_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "admin")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = await _sut.HasPermissionAsync("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAnyPermissionAsync_WithEmptyArray_ReturnsFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "admin")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = await _sut.HasAnyPermissionAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAllPermissionsAsync_WithEmptyArray_ReturnsTrue()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "admin")
        ], "Bearer");
        context.User = new ClaimsPrincipal(identity);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        // Act
        var result = await _sut.HasAllPermissionsAsync();

        // Assert
        result.Should().BeTrue(); // Empty set is considered "all satisfied"
    }
}
