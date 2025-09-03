using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Infrastructure.Persistence.Services;

/// <summary>
///     SQL Server implementation of database operations for tenant database management.
/// </summary>
public class SqlServerDatabaseOperations(ILogger<SqlServerDatabaseOperations> logger) : IDatabaseOperations
{
    /// <inheritdoc />
    public async Task<Result> CreateDatabaseAsync(string databaseName, string masterConnectionString)
    {
        try
        {
            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();
            var createDbCommand = $"CREATE DATABASE [{databaseName}]";

            await using var command = connection.CreateCommand();
            command.CommandText = createDbCommand;
            await command.ExecuteNonQueryAsync();

            logger.LogInformation("Successfully created database '{DatabaseName}'", databaseName);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating database '{DatabaseName}'", databaseName);
            return Result.Fail($"Failed to create database: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteDatabaseAsync(string databaseName, string masterConnectionString)
    {
        try
        {
            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            // First, close all connections to the database
            var closeConnectionsCommand = $"""
                                           ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                           DROP DATABASE [{databaseName}];
                                           """;

            await using var command = connection.CreateCommand();
            command.CommandText = closeConnectionsCommand;
            await command.ExecuteNonQueryAsync();

            logger.LogInformation("Successfully deleted database '{DatabaseName}'", databaseName);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting database '{DatabaseName}'", databaseName);
            return Result.Fail($"Failed to delete database: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> MigrateDatabaseAsync(string connectionString)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            await using var context = new TenantDbContext(optionsBuilder.Options);
            await context.Database.MigrateAsync();

            logger.LogInformation("Successfully migrated database");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error migrating database");
            return Result.Fail($"Failed to migrate database: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> BackupDatabaseAsync(string databaseName, string backupPath, string masterConnectionString)
    {
        try
        {
            var backupFileName = Path.Combine(backupPath, $"{databaseName}_{DateTime.UtcNow:yyyyMMddHHmmss}.bak");

            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            var backupCommand = $"""
                                 BACKUP DATABASE [{databaseName}]
                                 TO DISK = @BackupPath
                                 WITH FORMAT, INIT, NAME = @BackupName, SKIP, NOREWIND, NOUNLOAD, STATS = 10
                                 """;

            await using var command = connection.CreateCommand();
            command.CommandText = backupCommand;
            command.Parameters.AddWithValue("@BackupPath", backupFileName);
            command.Parameters.AddWithValue("@BackupName", $"{databaseName} Full Backup");
            await command.ExecuteNonQueryAsync();

            logger.LogInformation("Successfully backed up database '{DatabaseName}' to {BackupPath}", databaseName, backupFileName);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error backing up database '{DatabaseName}'", databaseName);
            return Result.Fail($"Failed to backup database: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> CheckDatabaseHealthAsync(string connectionString)
    {
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync();

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database health check failed");
            return Result.Ok(false);
        }
    }
}
