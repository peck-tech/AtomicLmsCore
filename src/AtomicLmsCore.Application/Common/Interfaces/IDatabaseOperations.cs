using FluentResults;

namespace AtomicLmsCore.Application.Common.Interfaces;

/// <summary>
/// Interface for abstracting low-level database operations for tenant database management.
/// This allows the TenantDatabaseService to be moved to the Application layer while keeping
/// infrastructure concerns in the Infrastructure layer.
/// </summary>
public interface IDatabaseOperations
{
    /// <summary>
    /// Creates a new database with the specified name.
    /// </summary>
    /// <param name="databaseName">The name of the database to create.</param>
    /// <param name="masterConnectionString">The connection string for the master database.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> CreateDatabaseAsync(string databaseName, string masterConnectionString);

    /// <summary>
    /// Deletes a database with the specified name.
    /// </summary>
    /// <param name="databaseName">The name of the database to delete.</param>
    /// <param name="masterConnectionString">The connection string for the master database.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteDatabaseAsync(string databaseName, string masterConnectionString);

    /// <summary>
    /// Runs Entity Framework migrations on the specified tenant database.
    /// </summary>
    /// <param name="connectionString">The connection string for the tenant database.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> MigrateDatabaseAsync(string connectionString);

    /// <summary>
    /// Creates a backup of the specified database.
    /// </summary>
    /// <param name="databaseName">The name of the database to backup.</param>
    /// <param name="backupPath">The file path where the backup should be saved.</param>
    /// <param name="masterConnectionString">The connection string for the master database.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> BackupDatabaseAsync(string databaseName, string backupPath, string masterConnectionString);

    /// <summary>
    /// Performs a health check on the specified database.
    /// </summary>
    /// <param name="connectionString">The connection string for the database to check.</param>
    /// <returns>A result indicating whether the database is healthy.</returns>
    Task<Result<bool>> CheckDatabaseHealthAsync(string connectionString);
}
