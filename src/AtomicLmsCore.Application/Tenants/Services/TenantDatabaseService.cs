using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Tenants.Services;

/// <summary>
///     Service for managing tenant database lifecycle operations.
/// </summary>
public class TenantDatabaseService(
    IConfiguration configuration,
    IConnectionStringProvider connectionStringProvider,
    ITenantRepository tenantRepository,
    ITenantDatabaseValidator tenantDatabaseValidator,
    IDatabaseOperations databaseOperations,
    ILogger<TenantDatabaseService> logger) : ITenantDatabaseService
{
    /// <inheritdoc />
    public async Task<Result> CreateTenantDatabaseAsync(Guid tenantId)
    {
        try
        {
            // Get tenant information to retrieve database name
            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                return Result.Fail($"Tenant with ID {tenantId} not found");
            }

            if (string.IsNullOrEmpty(tenant.DatabaseName))
            {
                return Result.Fail($"Tenant {tenantId} does not have a database name configured");
            }

            // Check if database already exists
            var databaseExists = await connectionStringProvider.TenantDatabaseExistsAsync(tenant.DatabaseName);
            if (databaseExists)
            {
                return Result.Fail($"Database '{tenant.DatabaseName}' for tenant {tenantId} already exists");
            }

            var masterConnectionString = configuration.GetConnectionString("MasterDatabase");
            if (string.IsNullOrEmpty(masterConnectionString))
            {
                return Result.Fail("Master database connection string not configured");
            }

            // Create the database
            var createResult = await databaseOperations.CreateDatabaseAsync(tenant.DatabaseName, masterConnectionString);
            if (createResult.IsFailed)
            {
                return Result.Fail($"Failed to create database: {string.Join(", ", createResult.Errors)}");
            }

            // Apply migrations to the new database
            var migrateResult = await MigrateTenantDatabaseAsync(tenantId);
            if (migrateResult.IsFailed)
            {
                // If migration fails, try to delete the database
                await DeleteTenantDatabaseAsync(tenantId);
                return Result.Fail($"Database created but migration failed: {string.Join(", ", migrateResult.Errors)}");
            }

            logger.LogInformation(
                "Successfully created database '{DatabaseName}' for tenant {TenantId}",
                tenant.DatabaseName,
                tenantId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating database for tenant {TenantId}", tenantId);
            return Result.Fail($"Failed to create tenant database: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteTenantDatabaseAsync(Guid tenantId)
    {
        try
        {
            // Get tenant information to retrieve database name
            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                logger.LogWarning("Attempted to delete database for non-existent tenant {TenantId}", tenantId);
                return Result.Ok(); // Consider this successful since the end state is achieved
            }

            if (string.IsNullOrEmpty(tenant.DatabaseName))
            {
                logger.LogWarning("Tenant {TenantId} does not have a database name configured", tenantId);
                return Result.Ok(); // Consider this successful since there's no database to delete
            }

            // Check if database exists before attempting to delete
            var databaseExists = await connectionStringProvider.TenantDatabaseExistsAsync(tenant.DatabaseName);
            if (!databaseExists)
            {
                logger.LogWarning(
                    "Attempted to delete non-existent database '{DatabaseName}' for tenant {TenantId}",
                    tenant.DatabaseName,
                    tenantId);
                return Result.Ok(); // Consider this successful since the end state is achieved
            }

            var masterConnectionString = configuration.GetConnectionString("MasterDatabase");
            if (string.IsNullOrEmpty(masterConnectionString))
            {
                return Result.Fail("Master database connection string not configured");
            }

            var deleteResult = await databaseOperations.DeleteDatabaseAsync(tenant.DatabaseName, masterConnectionString);
            if (deleteResult.IsFailed)
            {
                return Result.Fail($"Failed to delete database: {string.Join(", ", deleteResult.Errors)}");
            }

            logger.LogInformation(
                "Successfully deleted database '{DatabaseName}' for tenant {TenantId}",
                tenant.DatabaseName,
                tenantId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting database for tenant {TenantId}", tenantId);
            return Result.Fail($"Failed to delete tenant database: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> MigrateTenantDatabaseAsync(Guid tenantId)
    {
        try
        {
            // Get tenant information to retrieve database name
            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                return Result.Fail($"Tenant with ID {tenantId} not found");
            }

            if (string.IsNullOrEmpty(tenant.DatabaseName))
            {
                return Result.Fail($"Tenant {tenantId} does not have a database name configured");
            }

            var connectionString = connectionStringProvider.GetTenantConnectionString(tenant.DatabaseName);

            // Run the migration
            var migrateResult = await databaseOperations.MigrateDatabaseAsync(connectionString);
            if (migrateResult.IsFailed)
            {
                return Result.Fail($"Migration failed: {string.Join(", ", migrateResult.Errors)}");
            }

            // Create tenant identity record after successful migration
            var identityResult =
                await tenantDatabaseValidator.CreateTenantIdentityAsync(
                    tenantId,
                    tenant.DatabaseName,
                    connectionString);
            if (identityResult.IsFailed)
            {
                logger.LogError(
                    "Failed to create tenant identity after migration for tenant {TenantId}: {Errors}",
                    tenantId,
                    string.Join(", ", identityResult.Errors.Select(e => e.Message)));
                return Result.Fail($"Migration succeeded but failed to create tenant identity: {string.Join(", ", identityResult.Errors.Select(e => e.Message))}");
            }

            logger.LogInformation(
                "Successfully migrated database '{DatabaseName}' for tenant {TenantId} and created tenant identity",
                tenant.DatabaseName,
                tenantId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error migrating database for tenant {TenantId}", tenantId);
            return Result.Fail($"Failed to migrate tenant database: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result> BackupTenantDatabaseAsync(Guid tenantId, string backupPath)
    {
        try
        {
            // Get tenant information to retrieve database name
            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                return Result.Fail($"Tenant with ID {tenantId} not found");
            }

            if (string.IsNullOrEmpty(tenant.DatabaseName))
            {
                return Result.Fail($"Tenant {tenantId} does not have a database name configured");
            }

            var masterConnectionString = configuration.GetConnectionString("MasterDatabase");
            if (string.IsNullOrEmpty(masterConnectionString))
            {
                return Result.Fail("Master database connection string not configured");
            }

            var backupResult = await databaseOperations.BackupDatabaseAsync(tenant.DatabaseName, backupPath, masterConnectionString);
            if (backupResult.IsFailed)
            {
                return Result.Fail($"Failed to backup database: {string.Join(", ", backupResult.Errors)}");
            }

            logger.LogInformation("Successfully backed up database '{DatabaseName}' for tenant {TenantId}", tenant.DatabaseName, tenantId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error backing up database for tenant {TenantId}", tenantId);
            return Result.Fail($"Failed to backup tenant database: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> CheckTenantDatabaseHealthAsync(Guid tenantId)
    {
        try
        {
            // Get tenant information to retrieve database name
            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant == null)
            {
                logger.LogWarning("Database health check failed: tenant {TenantId} not found", tenantId);
                return Result.Ok(false);
            }

            if (string.IsNullOrEmpty(tenant.DatabaseName))
            {
                logger.LogWarning(
                    "Database health check failed: tenant {TenantId} does not have a database name configured",
                    tenantId);
                return Result.Ok(false);
            }

            var connectionString = connectionStringProvider.GetTenantConnectionString(tenant.DatabaseName);
            var healthResult = await databaseOperations.CheckDatabaseHealthAsync(connectionString);

            return healthResult;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database health check failed for tenant {TenantId}", tenantId);
            return Result.Ok(false);
        }
    }

    /// <inheritdoc />
    public async Task<Result<int>> MigrateAllTenantDatabasesAsync()
    {
        try
        {
            var tenants = await tenantRepository.GetAllAsync();
            var successCount = 0;
            var errors = new List<string>();

            foreach (var tenant in tenants)
            {
                var result = await MigrateTenantDatabaseAsync(tenant.Id);
                if (result.IsSuccess)
                {
                    successCount++;
                }
                else
                {
                    errors.Add($"Tenant {tenant.Id}: {string.Join(", ", result.Errors)}");
                }
            }

            if (errors.Any())
            {
                logger.LogWarning(
                    "Migrated {SuccessCount} databases with {ErrorCount} failures",
                    successCount,
                    errors.Count);
                return Result.Ok(successCount).WithErrors(errors.Select(e => new Error(e)));
            }

            logger.LogInformation("Successfully migrated all {Count} tenant databases", successCount);
            return Result.Ok(successCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error migrating all tenant databases");
            return Result.Fail<int>($"Failed to migrate all tenant databases: {ex.Message}");
        }
    }
}
