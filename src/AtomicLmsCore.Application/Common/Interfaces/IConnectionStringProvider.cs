namespace AtomicLmsCore.Application.Common.Interfaces;

/// <summary>
///     Provides connection strings for the multi-database architecture.
/// </summary>
public interface IConnectionStringProvider
{
    /// <summary>
    ///     Gets the connection string for the shared Solutions database.
    /// </summary>
    /// <returns>The Solutions database connection string.</returns>
    string GetSolutionsConnectionString();

    /// <summary>
    ///     Gets the connection string for a tenant's database by database name.
    /// </summary>
    /// <param name="databaseName">The name of the tenant's database.</param>
    /// <returns>The tenant-specific database connection string.</returns>
    string GetTenantConnectionString(string databaseName);

    /// <summary>
    ///     Checks if a tenant database exists.
    /// </summary>
    /// <param name="databaseName">The name of the tenant's database.</param>
    /// <returns>True if the tenant database exists, false otherwise.</returns>
    Task<bool> TenantDatabaseExistsAsync(string databaseName);
}
