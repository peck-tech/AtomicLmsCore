using FluentResults;

namespace AtomicLmsCore.Application.Common.Interfaces;

/// <summary>
///     Service for managing tenant database lifecycle operations.
/// </summary>
public interface ITenantDatabaseService
{
    /// <summary>
    ///     Creates a new database for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant's unique identifier.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> CreateTenantDatabaseAsync(Guid tenantId);

    /// <summary>
    ///     Deletes a tenant's database.
    /// </summary>
    /// <param name="tenantId">The tenant's unique identifier.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> DeleteTenantDatabaseAsync(Guid tenantId);

    /// <summary>
    ///     Applies pending migrations to a tenant's database.
    /// </summary>
    /// <param name="tenantId">The tenant's unique identifier.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> MigrateTenantDatabaseAsync(Guid tenantId);

    /// <summary>
    ///     Backs up a tenant's database.
    /// </summary>
    /// <param name="tenantId">The tenant's unique identifier.</param>
    /// <param name="backupPath">The path where the backup should be stored.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> BackupTenantDatabaseAsync(Guid tenantId, string backupPath);

    /// <summary>
    ///     Checks if a tenant's database exists and is accessible.
    /// </summary>
    /// <param name="tenantId">The tenant's unique identifier.</param>
    /// <returns>A result containing true if the database exists and is accessible, false otherwise.</returns>
    Task<Result<bool>> CheckTenantDatabaseHealthAsync(Guid tenantId);

    /// <summary>
    ///     Applies migrations to all tenant databases.
    /// </summary>
    /// <returns>A result containing the number of successful migrations and any errors.</returns>
    Task<Result<int>> MigrateAllTenantDatabasesAsync();
}
