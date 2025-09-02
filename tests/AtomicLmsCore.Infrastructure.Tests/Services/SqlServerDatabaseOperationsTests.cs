using AtomicLmsCore.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Infrastructure.Tests.Services;

public class SqlServerDatabaseOperationsTests
{
    private readonly Mock<ILogger<SqlServerDatabaseOperations>> _mockLogger;
    private readonly SqlServerDatabaseOperations _operations;

    public SqlServerDatabaseOperationsTests()
    {
        _mockLogger = new Mock<ILogger<SqlServerDatabaseOperations>>();
        _operations = new SqlServerDatabaseOperations(_mockLogger.Object);
    }

    [Fact]
    public async Task CreateDatabaseAsync_InvalidConnectionString_ReturnsFailure()
    {
        // Arrange
        var databaseName = "TestDb";
        var invalidConnectionString = "Invalid Connection String";

        // Act
        var result = await _operations.CreateDatabaseAsync(databaseName, invalidConnectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to create database:");
        VerifyLoggerWasCalled(LogLevel.Error, "Error creating database");
    }

    [Fact]
    public async Task DeleteDatabaseAsync_InvalidConnectionString_ReturnsFailure()
    {
        // Arrange
        var databaseName = "TestDb";
        var invalidConnectionString = "Invalid Connection String";

        // Act
        var result = await _operations.DeleteDatabaseAsync(databaseName, invalidConnectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to delete database:");
        VerifyLoggerWasCalled(LogLevel.Error, "Error deleting database");
    }

    [Fact]
    public async Task MigrateDatabaseAsync_InvalidConnectionString_ReturnsFailure()
    {
        // Arrange
        var invalidConnectionString = "Invalid Connection String";

        // Act
        var result = await _operations.MigrateDatabaseAsync(invalidConnectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to migrate database:");
        VerifyLoggerWasCalled(LogLevel.Error, "Error migrating database");
    }

    [Fact]
    public async Task BackupDatabaseAsync_InvalidConnectionString_ReturnsFailure()
    {
        // Arrange
        var databaseName = "TestDb";
        var backupPath = "/tmp/backups";
        var invalidConnectionString = "Invalid Connection String";

        // Act
        var result = await _operations.BackupDatabaseAsync(databaseName, backupPath, invalidConnectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to backup database:");
        VerifyLoggerWasCalled(LogLevel.Error, "Error backing up database");
    }

    [Fact]
    public async Task CheckDatabaseHealthAsync_InvalidConnectionString_ReturnsHealthyFalse()
    {
        // Arrange
        var invalidConnectionString = "Invalid Connection String";

        // Act
        var result = await _operations.CheckDatabaseHealthAsync(invalidConnectionString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
        VerifyLoggerWasCalled(LogLevel.Warning, "Database health check failed");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task CreateDatabaseAsync_InvalidDatabaseName_ReturnsFailure(string? databaseName)
    {
        // Arrange
        var connectionString = "Data Source=localhost;Initial Catalog=master;Integrated Security=true;";

        // Act
        var result = await _operations.CreateDatabaseAsync(databaseName!, connectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to create database:");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task DeleteDatabaseAsync_InvalidDatabaseName_ReturnsFailure(string? databaseName)
    {
        // Arrange
        var connectionString = "Data Source=localhost;Initial Catalog=master;Integrated Security=true;";

        // Act
        var result = await _operations.DeleteDatabaseAsync(databaseName!, connectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to delete database:");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task BackupDatabaseAsync_InvalidDatabaseName_ReturnsFailure(string? databaseName)
    {
        // Arrange
        var backupPath = "/tmp/backups";
        var connectionString = "Data Source=localhost;Initial Catalog=master;Integrated Security=true;";

        // Act
        var result = await _operations.BackupDatabaseAsync(databaseName!, backupPath, connectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to backup database:");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task BackupDatabaseAsync_InvalidBackupPath_ReturnsFailure(string? backupPath)
    {
        // Arrange
        var databaseName = "TestDb";
        var connectionString = "Data Source=localhost;Initial Catalog=master;Integrated Security=true;";

        // Act
        var result = await _operations.BackupDatabaseAsync(databaseName, backupPath!, connectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to backup database:");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task MigrateDatabaseAsync_NullOrEmptyConnectionString_ReturnsFailure(string? connectionString)
    {
        // Act
        var result = await _operations.MigrateDatabaseAsync(connectionString!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to migrate database:");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public async Task CheckDatabaseHealthAsync_NullOrEmptyConnectionString_ReturnsHealthyFalse(string? connectionString)
    {
        // Act
        var result = await _operations.CheckDatabaseHealthAsync(connectionString!);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
        VerifyLoggerWasCalled(LogLevel.Warning, "Database health check failed");
    }

    [Fact]
    public async Task CreateDatabaseAsync_ThrowsException_LogsErrorAndReturnsFailure()
    {
        // Arrange
        var databaseName = "TestDb";
        var connectionString = "Data Source=nonexistent-server;Initial Catalog=master;Connection Timeout=1;";

        // Act
        var result = await _operations.CreateDatabaseAsync(databaseName, connectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to create database:");
        VerifyLoggerWasCalled(LogLevel.Error, $"Error creating database '{databaseName}'");
    }

    [Fact]
    public async Task DeleteDatabaseAsync_ThrowsException_LogsErrorAndReturnsFailure()
    {
        // Arrange
        var databaseName = "TestDb";
        var connectionString = "Data Source=nonexistent-server;Initial Catalog=master;Connection Timeout=1;";

        // Act
        var result = await _operations.DeleteDatabaseAsync(databaseName, connectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to delete database:");
        VerifyLoggerWasCalled(LogLevel.Error, $"Error deleting database '{databaseName}'");
    }

    [Fact]
    public async Task BackupDatabaseAsync_ThrowsException_LogsErrorAndReturnsFailure()
    {
        // Arrange
        var databaseName = "TestDb";
        var backupPath = "/tmp/backups";
        var connectionString = "Data Source=nonexistent-server;Initial Catalog=master;Connection Timeout=1;";

        // Act
        var result = await _operations.BackupDatabaseAsync(databaseName, backupPath, connectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to backup database:");
        VerifyLoggerWasCalled(LogLevel.Error, $"Error backing up database '{databaseName}'");
    }

    [Fact]
    public async Task MigrateDatabaseAsync_ThrowsException_LogsErrorAndReturnsFailure()
    {
        // Arrange
        var connectionString = "Data Source=nonexistent-server;Initial Catalog=TestDb;Connection Timeout=1;";

        // Act
        var result = await _operations.MigrateDatabaseAsync(connectionString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().StartWith("Failed to migrate database:");
        VerifyLoggerWasCalled(LogLevel.Error, "Error migrating database");
    }

    private void VerifyLoggerWasCalled(LogLevel level, string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
