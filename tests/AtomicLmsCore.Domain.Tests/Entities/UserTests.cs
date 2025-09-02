using AtomicLmsCore.Domain.Entities;
using FluentAssertions;

namespace AtomicLmsCore.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_DefaultValues_SetsExpectedDefaults()
    {
        // Act
        var user = new User();

        // Assert
        user.ExternalUserId.Should().Be(string.Empty);
        user.Email.Should().Be(string.Empty);
        user.FirstName.Should().Be(string.Empty);
        user.LastName.Should().Be(string.Empty);
        user.DisplayName.Should().Be(string.Empty);
        user.TenantInternalId.Should().Be(0);
        user.Tenant.Should().BeNull();
        user.IsActive.Should().BeTrue();
        user.Metadata.Should().NotBeNull();
        user.Metadata.Should().BeEmpty();
        user.Id.Should().Be(Guid.Empty);
        user.InternalId.Should().Be(0);
        user.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ExternalUserId_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();
        var expectedExternalUserId = "auth0|123456789";

        // Act
        user.ExternalUserId = expectedExternalUserId;

        // Assert
        user.ExternalUserId.Should().Be(expectedExternalUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("auth0|123456")]
    [InlineData("google-oauth2|987654321")]
    [InlineData("very-long-external-user-id-that-might-come-from-some-identity-provider")]
    public void ExternalUserId_SetVariousValues_UpdatesCorrectly(string externalUserId)
    {
        // Arrange
        var user = new User();

        // Act
        user.ExternalUserId = externalUserId;

        // Assert
        user.ExternalUserId.Should().Be(externalUserId);
    }

    [Fact]
    public void Email_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();
        var expectedEmail = "test@example.com";

        // Act
        user.Email = expectedEmail;

        // Assert
        user.Email.Should().Be(expectedEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData("simple@example.com")]
    [InlineData("user.name+tag@example.co.uk")]
    [InlineData("very.long.email.address.with.many.dots@subdomain.example.org")]
    public void Email_SetVariousValues_UpdatesCorrectly(string email)
    {
        // Arrange
        var user = new User();

        // Act
        user.Email = email;

        // Assert
        user.Email.Should().Be(email);
    }

    [Fact]
    public void FirstName_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();
        var expectedFirstName = "John";

        // Act
        user.FirstName = expectedFirstName;

        // Assert
        user.FirstName.Should().Be(expectedFirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData("John")]
    [InlineData("Jean-Baptiste")]
    [InlineData("María José")]
    public void FirstName_SetVariousValues_UpdatesCorrectly(string firstName)
    {
        // Arrange
        var user = new User();

        // Act
        user.FirstName = firstName;

        // Assert
        user.FirstName.Should().Be(firstName);
    }

    [Fact]
    public void LastName_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();
        var expectedLastName = "Doe";

        // Act
        user.LastName = expectedLastName;

        // Assert
        user.LastName.Should().Be(expectedLastName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("O'Connor")]
    [InlineData("Van Der Berg")]
    [InlineData("Smith-Johnson")]
    [InlineData("García-López")]
    public void LastName_SetVariousValues_UpdatesCorrectly(string lastName)
    {
        // Arrange
        var user = new User();

        // Act
        user.LastName = lastName;

        // Assert
        user.LastName.Should().Be(lastName);
    }

    [Fact]
    public void DisplayName_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();
        var expectedDisplayName = "John D.";

        // Act
        user.DisplayName = expectedDisplayName;

        // Assert
        user.DisplayName.Should().Be(expectedDisplayName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("JD")]
    [InlineData("John Doe")]
    [InlineData("Prof. John Doe, Ph.D.")]
    public void DisplayName_SetVariousValues_UpdatesCorrectly(string displayName)
    {
        // Arrange
        var user = new User();

        // Act
        user.DisplayName = displayName;

        // Assert
        user.DisplayName.Should().Be(displayName);
    }

    [Fact]
    public void TenantInternalId_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();
        var expectedTenantInternalId = 42;

        // Act
        user.TenantInternalId = expectedTenantInternalId;

        // Assert
        user.TenantInternalId.Should().Be(expectedTenantInternalId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(999)]
    [InlineData(int.MaxValue)]
    public void TenantInternalId_SetVariousValues_UpdatesCorrectly(int tenantInternalId)
    {
        // Arrange
        var user = new User();

        // Act
        user.TenantInternalId = tenantInternalId;

        // Assert
        user.TenantInternalId.Should().Be(tenantInternalId);
    }

    [Fact]
    public void Tenant_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();
        var expectedTenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant" };

        // Act
        user.Tenant = expectedTenant;

        // Assert
        user.Tenant.Should().Be(expectedTenant);
    }

    [Fact]
    public void IsActive_SetFalse_UpdatesCorrectly()
    {
        // Arrange
        var user = new User(); // Default is true

        // Act
        user.IsActive = false;

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_SetTrue_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();
        user.IsActive = false; // Set to false first

        // Act
        user.IsActive = true;

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Metadata_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();
        var expectedMetadata = new Dictionary<string, string>
        {
            { "department", "Engineering" },
            { "role", "Senior Developer" }
        };

        // Act
        user.Metadata = expectedMetadata;

        // Assert
        user.Metadata.Should().BeEquivalentTo(expectedMetadata);
    }

    [Fact]
    public void Metadata_AddItems_UpdatesCollection()
    {
        // Arrange
        var user = new User();

        // Act
        user.Metadata["department"] = "HR";
        user.Metadata["startDate"] = "2023-01-15";
        user.Metadata["manager"] = "Jane Smith";

        // Assert
        user.Metadata.Should().HaveCount(3);
        user.Metadata["department"].Should().Be("HR");
        user.Metadata["startDate"].Should().Be("2023-01-15");
        user.Metadata["manager"].Should().Be("Jane Smith");
    }

    [Fact]
    public void Metadata_ModifyExistingItems_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();
        user.Metadata["status"] = "pending";

        // Act
        user.Metadata["status"] = "active";

        // Assert
        user.Metadata["status"].Should().Be("active");
        user.Metadata.Should().HaveCount(1);
    }

    [Fact]
    public void User_InheritsFromBaseEntity()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void AllProperties_SetSimultaneously_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant" };
        var metadata = new Dictionary<string, string> { { "key", "value" } };

        // Act
        user.ExternalUserId = "auth0|123";
        user.Email = "test@example.com";
        user.FirstName = "John";
        user.LastName = "Doe";
        user.DisplayName = "John D.";
        user.TenantInternalId = 1;
        user.Tenant = tenant;
        user.IsActive = false;
        user.Metadata = metadata;

        // Assert
        user.ExternalUserId.Should().Be("auth0|123");
        user.Email.Should().Be("test@example.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.DisplayName.Should().Be("John D.");
        user.TenantInternalId.Should().Be(1);
        user.Tenant.Should().Be(tenant);
        user.IsActive.Should().BeFalse();
        user.Metadata.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public void PropertyAssignments_IndependentOfEachOther_UpdatesCorrectly()
    {
        // Arrange
        var user = new User();

        // Act & Assert - Test that setting one property doesn't affect others
        user.FirstName = "John";
        user.LastName.Should().Be(string.Empty);
        user.Email.Should().Be(string.Empty);

        user.Email = "john@example.com";
        user.ExternalUserId.Should().Be(string.Empty);
        user.DisplayName.Should().Be(string.Empty);

        user.IsActive = false;
        user.TenantInternalId.Should().Be(0);
        user.FirstName.Should().Be("John");
        user.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void Metadata_HandleSpecialCharacters_StoresCorrectly()
    {
        // Arrange
        var user = new User();
        var specialKey = "pref-with_special.chars@123";
        var specialValue = "Value with spaces, punctuation! & symbols #123";

        // Act
        user.Metadata[specialKey] = specialValue;

        // Assert
        user.Metadata[specialKey].Should().Be(specialValue);
    }

    [Fact]
    public void Metadata_CaseSensitivity_TreatsKeysAsCaseSensitive()
    {
        // Arrange
        var user = new User();

        // Act
        user.Metadata["Department"] = "Engineering";
        user.Metadata["department"] = "Sales";
        user.Metadata["DEPARTMENT"] = "Marketing";

        // Assert
        user.Metadata.Should().HaveCount(3);
        user.Metadata["Department"].Should().Be("Engineering");
        user.Metadata["department"].Should().Be("Sales");
        user.Metadata["DEPARTMENT"].Should().Be("Marketing");
    }
}
