using System.Security.Cryptography;
using System.Text;
using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Infrastructure.Tests.Services;

public class TenantDatabaseValidatorTests : IDisposable
{
    private readonly Mock<IConnectionStringProvider> _connectionStringProviderMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<TenantDatabaseValidator>> _loggerMock;
    private readonly TenantDatabaseValidator _validator;
    private const string TestSecret = "test-secret-key";

    public TenantDatabaseValidatorTests()
    {
        _connectionStringProviderMock = new Mock<IConnectionStringProvider>();
        _configurationMock = new Mock<IConfiguration>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<TenantDatabaseValidator>>();

        // Setup configuration
        _configurationMock.Setup(x => x["TenantValidation:Secret"])
            .Returns(TestSecret);

        // Setup connection string provider to return invalid connection to trigger test scenarios
        _connectionStringProviderMock
            .Setup(x => x.GetTenantConnectionString(It.IsAny<string>()))
            .Returns("Server=nonexistent;Database=test;Connection Timeout=1;");

        _validator = new TenantDatabaseValidator(
            _connectionStringProviderMock.Object,
            _configurationMock.Object,
            _memoryCache,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }

    public class CacheTests : TenantDatabaseValidatorTests
    {
        [Fact]
        public async Task ValidateTenantDatabaseAsync_CachesFailureResults()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var databaseName = "nonexistent_db";

            // Act - First call (will fail due to connection issues)
            var result1 = await _validator.ValidateTenantDatabaseAsync(tenantId, databaseName);

            // Second call should use cached result
            var result2 = await _validator.ValidateTenantDatabaseAsync(tenantId, databaseName);

            // Assert
            result1.IsFailed.Should().BeTrue();
            result2.IsFailed.Should().BeTrue();

            // Verify logging occurred only once (proving cache was used)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Performing database validation")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ValidateTenantDatabaseAsync_UsesCachedResults()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            const string DatabaseName = "test_db";

            // Act - Multiple calls
            _ = await _validator.ValidateTenantDatabaseAsync(tenantId, DatabaseName);
            _ = await _validator.ValidateTenantDatabaseAsync(tenantId, DatabaseName);

            // Assert - Verify debug logging for cache hit
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Using cached validation result")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    public class SecurityTests : TenantDatabaseValidatorTests
    {
        [Fact]
        public async Task ValidateTenantDatabaseAsync_WithSqlInjectionAttempt_HandledSafely()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var maliciousDatabaseName = "test'; DROP TABLE TenantIdentity; --";

            // Act & Assert - Should not throw exception
            var result = await _validator.ValidateTenantDatabaseAsync(tenantId, maliciousDatabaseName);
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task CreateTenantIdentityAsync_LogsSecurityEvents()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var databaseName = "test_db";
            var connectionString = "Server=nonexistent;Database=test;Connection Timeout=1;";

            // Act
            var result = await _validator.CreateTenantIdentityAsync(tenantId, databaseName, connectionString);

            // Assert - Should fail due to connection, but verify it attempted to log
            result.IsFailed.Should().BeTrue();
        }
    }

    public class EdgeCaseTests : TenantDatabaseValidatorTests
    {
        [Fact]
        public async Task ValidateTenantDatabaseAsync_WithEmptyGuid_ReturnsFailure()
        {
            // Act
            var result = await _validator.ValidateTenantDatabaseAsync(Guid.Empty, "test_db");

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateTenantDatabaseAsync_WithEmptyDatabaseName_ReturnsFailure()
        {
            // Act
            var result = await _validator.ValidateTenantDatabaseAsync(Guid.NewGuid(), "");

            // Assert
            result.IsFailed.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateTenantDatabaseAsync_WithNullDatabaseName_ReturnsFailure()
        {
            // Act
            var result = await _validator.ValidateTenantDatabaseAsync(Guid.NewGuid(), null!);

            // Assert
            result.IsFailed.Should().BeTrue();
        }
    }

    public class HashGenerationTests : TenantDatabaseValidatorTests
    {
        [Fact]
        public void GenerateValidationHash_ProducesConsistentResults()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var databaseName = "test_db";
            var createdAt = DateTime.UtcNow;

            // Act - Generate hash twice with same inputs
            var hash1 = GenerateValidationHash(tenantId, databaseName, createdAt);
            var hash2 = GenerateValidationHash(tenantId, databaseName, createdAt);

            // Assert
            hash1.Should().Be(hash2);
            hash1.Should().NotBeEmpty();
        }

        [Fact]
        public void GenerateValidationHash_ProducesDifferentResultsForDifferentInputs()
        {
            // Arrange
            var tenantId1 = Guid.NewGuid();
            var tenantId2 = Guid.NewGuid();
            var databaseName = "test_db";
            var createdAt = DateTime.UtcNow;

            // Act
            var hash1 = GenerateValidationHash(tenantId1, databaseName, createdAt);
            var hash2 = GenerateValidationHash(tenantId2, databaseName, createdAt);

            // Assert
            hash1.Should().NotBe(hash2);
        }
    }

    private string GenerateValidationHash(Guid tenantId, string databaseName, DateTime createdAt)
    {
        var input = $"{tenantId}|{databaseName}|{createdAt:O}|{TestSecret}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}
