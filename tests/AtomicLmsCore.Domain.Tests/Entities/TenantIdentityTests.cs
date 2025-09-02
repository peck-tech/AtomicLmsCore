using AtomicLmsCore.Domain.Entities;
using FluentAssertions;

namespace AtomicLmsCore.Domain.Tests.Entities;

public class TenantIdentityTests
{
    [Fact]
    public void Constructor_DefaultValues_SetsExpectedDefaults()
    {
        // Act
        var tenantIdentity = new TenantIdentity();

        // Assert
        tenantIdentity.TenantId.Should().Be(Guid.Empty);
        tenantIdentity.DatabaseName.Should().Be(string.Empty);
        tenantIdentity.CreatedAt.Should().Be(DateTime.MinValue);
        tenantIdentity.ValidationHash.Should().Be(string.Empty);
        tenantIdentity.CreationMetadata.Should().Be(string.Empty);
    }

    [Fact]
    public void TenantId_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();
        var expectedTenantId = Guid.NewGuid();

        // Act
        tenantIdentity.TenantId = expectedTenantId;

        // Assert
        tenantIdentity.TenantId.Should().Be(expectedTenantId);
    }

    [Fact]
    public void DatabaseName_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();
        var expectedDatabaseName = "TenantDatabase_ABC123";

        // Act
        tenantIdentity.DatabaseName = expectedDatabaseName;

        // Assert
        tenantIdentity.DatabaseName.Should().Be(expectedDatabaseName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("SimpleDb")]
    [InlineData("TenantDatabase_WithSpecialChars-123.Test")]
    [InlineData("VeryLongDatabaseNameThatShouldStillBeHandledCorrectlyWithoutIssues")]
    public void DatabaseName_SetVariousValues_UpdatesCorrectly(string databaseName)
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();

        // Act
        tenantIdentity.DatabaseName = databaseName;

        // Assert
        tenantIdentity.DatabaseName.Should().Be(databaseName);
    }

    [Fact]
    public void CreatedAt_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();
        var expectedCreatedAt = DateTime.UtcNow;

        // Act
        tenantIdentity.CreatedAt = expectedCreatedAt;

        // Assert
        tenantIdentity.CreatedAt.Should().Be(expectedCreatedAt);
    }

    [Fact]
    public void ValidationHash_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();
        var expectedValidationHash = "abc123def456ghi789jkl012";

        // Act
        tenantIdentity.ValidationHash = expectedValidationHash;

        // Assert
        tenantIdentity.ValidationHash.Should().Be(expectedValidationHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0")]
    [InlineData("hash-with-special-characters!@#$%^&*()")]
    public void ValidationHash_SetVariousValues_UpdatesCorrectly(string validationHash)
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();

        // Act
        tenantIdentity.ValidationHash = validationHash;

        // Assert
        tenantIdentity.ValidationHash.Should().Be(validationHash);
    }

    [Fact]
    public void CreationMetadata_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();
        var expectedMetadata = "Created by automated provisioning system v2.1.0";

        // Act
        tenantIdentity.CreationMetadata = expectedMetadata;

        // Assert
        tenantIdentity.CreationMetadata.Should().Be(expectedMetadata);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Simple metadata")]
    [InlineData("Complex metadata with special characters: !@#$%^&*()_+{}|:<>?")]
    [InlineData("Very long metadata that contains multiple sentences and should be handled correctly without any issues regardless of the length.")]
    public void CreationMetadata_SetVariousValues_UpdatesCorrectly(string metadata)
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();

        // Act
        tenantIdentity.CreationMetadata = metadata;

        // Assert
        tenantIdentity.CreationMetadata.Should().Be(metadata);
    }

    [Fact]
    public void AllProperties_SetSimultaneously_UpdatesCorrectly()
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();
        var expectedTenantId = Guid.NewGuid();
        var expectedDatabaseName = "TestDatabase";
        var expectedCreatedAt = DateTime.UtcNow;
        var expectedValidationHash = "testHash123";
        var expectedMetadata = "Test metadata";

        // Act
        tenantIdentity.TenantId = expectedTenantId;
        tenantIdentity.DatabaseName = expectedDatabaseName;
        tenantIdentity.CreatedAt = expectedCreatedAt;
        tenantIdentity.ValidationHash = expectedValidationHash;
        tenantIdentity.CreationMetadata = expectedMetadata;

        // Assert
        tenantIdentity.TenantId.Should().Be(expectedTenantId);
        tenantIdentity.DatabaseName.Should().Be(expectedDatabaseName);
        tenantIdentity.CreatedAt.Should().Be(expectedCreatedAt);
        tenantIdentity.ValidationHash.Should().Be(expectedValidationHash);
        tenantIdentity.CreationMetadata.Should().Be(expectedMetadata);
    }

    [Fact]
    public void CreatedAt_SetPastDate_UpdatesCorrectly()
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();
        var pastDate = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        tenantIdentity.CreatedAt = pastDate;

        // Assert
        tenantIdentity.CreatedAt.Should().Be(pastDate);
    }

    [Fact]
    public void CreatedAt_SetFutureDate_UpdatesCorrectly()
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();
        var futureDate = DateTime.UtcNow.AddYears(1);

        // Act
        tenantIdentity.CreatedAt = futureDate;

        // Assert
        tenantIdentity.CreatedAt.Should().Be(futureDate);
    }

    [Fact]
    public void TenantId_SetEmptyGuid_UpdatesCorrectly()
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();
        tenantIdentity.TenantId = Guid.NewGuid(); // Set to non-empty first

        // Act
        tenantIdentity.TenantId = Guid.Empty;

        // Assert
        tenantIdentity.TenantId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TenantIdentity_DoesNotInheritFromBaseEntity()
    {
        // Arrange & Act
        var tenantIdentity = new TenantIdentity();

        // Assert
        tenantIdentity.Should().NotBeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void PropertyAssignments_IndependentOfEachOther_UpdatesCorrectly()
    {
        // Arrange
        var tenantIdentity = new TenantIdentity();

        // Act & Assert - Test that setting one property doesn't affect others
        tenantIdentity.TenantId = Guid.NewGuid();
        tenantIdentity.DatabaseName.Should().Be(string.Empty);
        tenantIdentity.ValidationHash.Should().Be(string.Empty);

        tenantIdentity.DatabaseName = "TestDb";
        tenantIdentity.CreationMetadata.Should().Be(string.Empty);
        tenantIdentity.CreatedAt.Should().Be(DateTime.MinValue);

        tenantIdentity.ValidationHash = "hash123";
        tenantIdentity.TenantId.Should().NotBe(Guid.Empty);
        tenantIdentity.DatabaseName.Should().Be("TestDb");
    }
}
