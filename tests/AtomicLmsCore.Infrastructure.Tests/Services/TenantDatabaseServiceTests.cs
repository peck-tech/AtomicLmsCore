using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Tenants.Services;
using AtomicLmsCore.Domain.Entities;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Infrastructure.Tests.Services;

public class TenantDatabaseServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IConnectionStringProvider> _connectionStringProviderMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<ITenantDatabaseValidator> _tenantDatabaseValidatorMock;
    private readonly Mock<IDatabaseOperations> _databaseOperationsMock;
    private readonly Mock<ILogger<TenantDatabaseService>> _loggerMock;
    private readonly TenantDatabaseService _service;

    public TenantDatabaseServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _connectionStringProviderMock = new Mock<IConnectionStringProvider>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _tenantDatabaseValidatorMock = new Mock<ITenantDatabaseValidator>();
        _databaseOperationsMock = new Mock<IDatabaseOperations>();
        _loggerMock = new Mock<ILogger<TenantDatabaseService>>();

        _service = new TenantDatabaseService(
            _configurationMock.Object,
            _connectionStringProviderMock.Object,
            _tenantRepositoryMock.Object,
            _tenantDatabaseValidatorMock.Object,
            _databaseOperationsMock.Object,
            _loggerMock.Object);
    }

    public class MigrateTenantDatabaseAsyncTests : TenantDatabaseServiceTests
    {
        [Fact]
        public async Task MigrateTenantDatabaseAsync_WhenTenantNotFound_ReturnsFailure()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            _tenantRepositoryMock
                .Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tenant?)null);

            // Act
            var result = await _service.MigrateTenantDatabaseAsync(tenantId);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Contain($"Tenant with ID {tenantId} not found");

            // Verify identity creation was NOT attempted
            _tenantDatabaseValidatorMock.Verify(
                x => x.CreateTenantIdentityAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task MigrateTenantDatabaseAsync_WhenDatabaseNameEmpty_ReturnsFailure()
        {
            // Arrange
            var tenantId = Guid.NewGuid();

            var tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Slug = "test-tenant",
                DatabaseName = "", // Empty database name
                IsActive = true
            };

            _tenantRepositoryMock
                .Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            // Act
            var result = await _service.MigrateTenantDatabaseAsync(tenantId);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Contain($"Tenant {tenantId} does not have a database name configured");

            // Verify identity creation was NOT attempted
            _tenantDatabaseValidatorMock.Verify(
                x => x.CreateTenantIdentityAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task MigrateTenantDatabaseAsync_WhenIdentityCreationFails_ReturnsFailure()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var databaseName = "test_tenant_db";
            var connectionString = "Server=test;Database=test;";

            var tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Slug = "test-tenant",
                DatabaseName = databaseName,
                IsActive = true
            };

            _tenantRepositoryMock
                .Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            _connectionStringProviderMock
                .Setup(x => x.GetTenantConnectionString(databaseName))
                .Returns(connectionString);

            _databaseOperationsMock
                .Setup(x => x.MigrateDatabaseAsync(connectionString))
                .ReturnsAsync(Result.Ok());

            _tenantDatabaseValidatorMock
                .Setup(x => x.CreateTenantIdentityAsync(tenantId, databaseName, connectionString))
                .ReturnsAsync(Result.Fail("Identity creation failed"));

            // Act
            var result = await _service.MigrateTenantDatabaseAsync(tenantId);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().Contain("Migration succeeded but failed to create tenant identity");
            result.Errors[0].Message.Should().Contain("Identity creation failed");

            // Verify identity creation was attempted
            _tenantDatabaseValidatorMock.Verify(
                x => x.CreateTenantIdentityAsync(tenantId, databaseName, connectionString),
                Times.Once);
        }
    }

    public class SecurityValidationTests : TenantDatabaseServiceTests
    {
        [Fact]
        public async Task CreateTenantDatabaseAsync_AlwaysAttemptsIdentityCreation()
        {
            // This test ensures that tenant identity creation is ALWAYS attempted when a database is created
            // Arrange
            var tenantId = Guid.NewGuid();
            var databaseName = "test_tenant_db";
            var connectionString = "Server=test;Database=test;";

            var tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Slug = "test-tenant",
                DatabaseName = databaseName,
                IsActive = true
            };

            _tenantRepositoryMock
                .Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            _connectionStringProviderMock
                .Setup(x => x.TenantDatabaseExistsAsync(databaseName))
                .ReturnsAsync(false);

            _connectionStringProviderMock
                .Setup(x => x.GetTenantConnectionString(databaseName))
                .Returns(connectionString);

            // Mock the connection strings configuration section
            var connectionStringsMock = new Mock<IConfigurationSection>();
            connectionStringsMock.SetupGet(x => x["MasterDatabase"]).Returns("Server=master;Database=master;");
            _configurationMock.Setup(x => x.GetSection("ConnectionStrings")).Returns(connectionStringsMock.Object);

            _databaseOperationsMock
                .Setup(x => x.CreateDatabaseAsync(databaseName, "Server=master;Database=master;"))
                .ReturnsAsync(Result.Ok());

            _databaseOperationsMock
                .Setup(x => x.MigrateDatabaseAsync(connectionString))
                .ReturnsAsync(Result.Ok());

            _tenantDatabaseValidatorMock
                .Setup(x => x.CreateTenantIdentityAsync(tenantId, databaseName, connectionString))
                .ReturnsAsync(Result.Ok());

            // Act
            _ = await _service.CreateTenantDatabaseAsync(tenantId);

            // Assert
            // Verify identity creation was attempted (regardless of other failures)
            _tenantDatabaseValidatorMock.Verify(
                x => x.CreateTenantIdentityAsync(tenantId, databaseName, connectionString),
                Times.Once,
                "Tenant identity creation MUST be attempted for every new tenant database");
        }

        [Fact]
        public async Task MigrateTenantDatabaseAsync_LogsErrorWhenIdentityCreationFails()
        {
            // This ensures security failures are properly logged for audit
            // Arrange
            var tenantId = Guid.NewGuid();
            var databaseName = "test_tenant_db";
            var connectionString = "Server=test;Database=test;";
            var identityError = "Security validation failed: Hash mismatch";

            var tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Slug = "test-tenant",
                DatabaseName = databaseName,
                IsActive = true
            };

            _tenantRepositoryMock
                .Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            _connectionStringProviderMock
                .Setup(x => x.GetTenantConnectionString(databaseName))
                .Returns(connectionString);

            _databaseOperationsMock
                .Setup(x => x.MigrateDatabaseAsync(connectionString))
                .ReturnsAsync(Result.Ok());

            _tenantDatabaseValidatorMock
                .Setup(x => x.CreateTenantIdentityAsync(tenantId, databaseName, connectionString))
                .ReturnsAsync(Result.Fail(identityError));

            // Act
            var result = await _service.MigrateTenantDatabaseAsync(tenantId);

            // Assert
            result.IsFailed.Should().BeTrue();

            // Verify error was logged for security audit
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create tenant identity")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Security validation failures MUST be logged for audit");
        }

        [Fact]
        public async Task MigrateTenantDatabaseAsync_WithValidInputs_AttemptsIdentityCreation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var databaseName = "valid_tenant_db";
            var connectionString = "Server=test;Database=test;";

            var tenant = new Tenant
            {
                Id = tenantId,
                Name = "Valid Tenant",
                Slug = "valid-tenant",
                DatabaseName = databaseName,
                IsActive = true
            };

            _tenantRepositoryMock
                .Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            _connectionStringProviderMock
                .Setup(x => x.GetTenantConnectionString(databaseName))
                .Returns(connectionString);

            _databaseOperationsMock
                .Setup(x => x.MigrateDatabaseAsync(connectionString))
                .ReturnsAsync(Result.Ok());

            _tenantDatabaseValidatorMock
                .Setup(x => x.CreateTenantIdentityAsync(tenantId, databaseName, connectionString))
                .ReturnsAsync(Result.Ok());

            // Act
            _ = await _service.MigrateTenantDatabaseAsync(tenantId);

            // Assert - Verification of security behavior
            _tenantDatabaseValidatorMock.Verify(
                x => x.CreateTenantIdentityAsync(
                    It.Is<Guid>(g => g == tenantId),
                    It.Is<string>(s => s == databaseName),
                    It.Is<string>(s => s == connectionString)),
                Times.Once,
                "Security validation MUST be performed with correct parameters");
        }
    }
}
