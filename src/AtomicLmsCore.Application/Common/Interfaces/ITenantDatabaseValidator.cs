using FluentResults;

namespace AtomicLmsCore.Application.Common.Interfaces;

/// <summary>
///     Service for validating that tenant databases actually belong to the correct tenant.
///     Provides cached validation to prevent performance impact.
/// </summary>
public interface ITenantDatabaseValidator
{
    /// <summary>
    ///     Validates that the specified database actually belongs to the given tenant.
    ///     Uses caching to minimize performance impact.
    /// </summary>
    /// <param name="tenantId">The tenant ID that should own the database.</param>
    /// <param name="databaseName">The name of the database to validate.</param>
    /// <returns>A result indicating whether validation passed or failed with error details.</returns>
    Task<Result> ValidateTenantDatabaseAsync(Guid tenantId, string databaseName);

    /// <summary>
    ///     Creates the tenant identity record in a newly created tenant database.
    ///     This establishes the cryptographic proof of tenant ownership.
    /// </summary>
    /// <param name="tenantId">The tenant ID that owns the database.</param>
    /// <param name="databaseName">The name of the database.</param>
    /// <param name="connectionString">The connection string to the tenant database.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> CreateTenantIdentityAsync(Guid tenantId, string databaseName, string connectionString);

    /// <summary>
    ///     Gets a validated connection string for a tenant database.
    ///     Performs validation if not already cached, then returns the connection string.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="databaseName">The database name.</param>
    /// <returns>A result containing the validated connection string or validation errors.</returns>
    Task<Result<string>> GetValidatedConnectionStringAsync(Guid tenantId, string databaseName);

    /// <summary>
    ///     Clears the validation cache for a specific tenant.
    ///     Use when tenant database configuration changes.
    /// </summary>
    /// <param name="tenantId">The tenant ID to clear from cache.</param>
    void ClearValidationCache(Guid tenantId);

    /// <summary>
    ///     Clears all validation caches.
    ///     Use for administrative purposes or testing.
    /// </summary>
    void ClearAllValidationCaches();
}
