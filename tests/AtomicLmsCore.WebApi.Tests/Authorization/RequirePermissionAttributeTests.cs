using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.WebApi.Authorization;
using FluentAssertions;

namespace AtomicLmsCore.WebApi.Tests.Authorization;

public class RequirePermissionAttributeTests
{
    [Fact]
    public void Constructor_WithSinglePermission_SetsPermissionsCorrectly()
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute("users:read");

        // Assert
        attribute.Permissions.Should().ContainSingle("users:read");
        attribute.RequireAll.Should().BeFalse();
        attribute.Policy.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithMultiplePermissions_SetsPermissionsCorrectly()
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute("users:read", "users:write");

        // Assert
        attribute.Permissions.Should().HaveCount(2);
        attribute.Permissions.Should().Contain("users:read");
        attribute.Permissions.Should().Contain("users:write");
        attribute.RequireAll.Should().BeFalse();
        attribute.Policy.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithNullPermissions_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var act = () => new RequirePermissionAttribute((string[])null!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("At least one permission must be specified*");
    }

    [Fact]
    public void Constructor_WithEmptyPermissions_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var act = () => new RequirePermissionAttribute();
        act.Should().Throw<ArgumentException>()
            .WithMessage("At least one permission must be specified*");
    }

    [Fact]
    public void RequireAll_WhenSet_UpdatesPolicyName()
    {
        // Arrange
        var attribute = new RequirePermissionAttribute("users:read", "users:write");
        var originalPolicy = attribute.Policy;

        // Act
        attribute.RequireAll = true;
        attribute.UpdatePolicy();

        // Assert
        attribute.Policy.Should().NotBe(originalPolicy);
        attribute.Policy.Should().Contain("ALL");
    }

    [Fact]
    public void Policy_WithSamePermissionsDifferentOrder_GeneratesSamePolicy()
    {
        // Arrange
        var attribute1 = new RequirePermissionAttribute("users:read", "users:write");
        var attribute2 = new RequirePermissionAttribute("users:write", "users:read");

        // Act & Assert
        attribute1.Policy.Should().Be(attribute2.Policy);
    }

    [Fact]
    public void Policy_WithDifferentRequireAll_GeneratesDifferentPolicies()
    {
        // Arrange
        var attribute1 = new RequirePermissionAttribute("users:read") { RequireAll = false };
        var attribute2 = new RequirePermissionAttribute("users:read") { RequireAll = true };

        attribute1.UpdatePolicy();
        attribute2.UpdatePolicy();

        // Act & Assert
        attribute1.Policy.Should().NotBe(attribute2.Policy);
        attribute1.Policy.Should().Contain("ANY");
        attribute2.Policy.Should().Contain("ALL");
    }

    [Fact]
    public void Policy_WithPredefinedPermissions_GeneratesExpectedFormat()
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute(Permissions.Users.Read);

        // Assert
        attribute.Policy.Should().StartWith("Permission_ANY_");
        attribute.Policy.Should().NotContain("="); // Base64 padding removed
        attribute.Policy.Should().NotContain("+"); // Base64 chars replaced
        attribute.Policy.Should().NotContain("/"); // Base64 chars replaced
    }

    [Theory]
    [InlineData("users:read")]
    [InlineData("tenants:manage")]
    [InlineData("system:admin")]
    public void Constructor_WithValidPermission_CreatesValidPolicy(string permission)
    {
        // Arrange & Act
        var attribute = new RequirePermissionAttribute(permission);

        // Assert
        attribute.Permissions.Should().ContainSingle(permission);
        attribute.Policy.Should().NotBeEmpty();
        attribute.Policy.Should().StartWith("Permission_");
    }

    [Fact]
    public void Attribute_CanBeAppliedToClass()
    {
        // This test verifies the attribute can be used on classes
        // Arrange & Act
        var attributeUsage = typeof(RequirePermissionAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        // Assert
        attributeUsage.Should().NotBeNull();
        attributeUsage!.ValidOn.Should().HaveFlag(AttributeTargets.Class);
        attributeUsage.ValidOn.Should().HaveFlag(AttributeTargets.Method);
        attributeUsage.AllowMultiple.Should().BeTrue();
    }
}
