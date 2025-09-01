using System.Security.Cryptography;
using System.Text;
using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Infrastructure.Persistence;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Infrastructure.Services;

/// <summary>
///     Service for validating that tenant databases actually belong to the correct tenant.
///     Uses caching to minimize performance impact of validation checks.
/// </summary>
public class TenantDatabaseValidator(
    IConnectionStringProvider connectionStringProvider,
    IConfiguration configuration,
    IMemoryCache cache,
    ILogger<TenantDatabaseValidator> logger) : ITenantDatabaseValidator
{
    private const string CacheKeyPrefix = "tenant_db_validation_";
    private static readonly TimeSpan ValidationCacheExpiry = TimeSpan.FromHours(1);

    /// <inheritdoc />
    public async Task<Result> ValidateTenantDatabaseAsync(Guid tenantId, string databaseName)
    {
        var cacheKey = GetCacheKey(tenantId, databaseName);

        // Check if validation is already cached
        if (cache.TryGetValue(cacheKey, out var cachedResult) && cachedResult is Result result)
        {
            logger.LogDebug(
                "Using cached validation result for tenant {TenantId} database {DatabaseName}",
                tenantId,
                databaseName);
            return result;
        }

        logger.LogInformation(
            "Performing database validation for tenant {TenantId} database {DatabaseName}",
            tenantId,
            databaseName);

        // Perform actual validation
        var validationResult = await PerformValidationAsync(tenantId, databaseName);

        // Cache the result (both success and failure for a shorter period)
        var cacheExpiry = validationResult.IsSuccess ? ValidationCacheExpiry : TimeSpan.FromMinutes(5);
        cache.Set(cacheKey, validationResult, cacheExpiry);

        return validationResult;
    }

    /// <inheritdoc />
    public async Task<Result> CreateTenantIdentityAsync(Guid tenantId, string databaseName, string connectionString)
    {
        try
        {
            var options = new DbContextOptionsBuilder<TenantDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var context = new TenantDbContext(options);

            // Check if identity record already exists
            var existingIdentity = await context.TenantIdentity
                .FirstOrDefaultAsync(ti => ti.TenantId == tenantId);

            if (existingIdentity != null)
            {
                logger.LogWarning(
                    "Tenant identity already exists for tenant {TenantId} in database {DatabaseName}",
                    tenantId,
                    databaseName);
                return Result.Fail($"Tenant identity already exists for tenant {tenantId}");
            }

            // Create new tenant identity record
            var createdAt = DateTime.UtcNow;
            var validationHash = GenerateValidationHash(tenantId, databaseName, createdAt);

            var tenantIdentity = new TenantIdentity
            {
                TenantId = tenantId,
                DatabaseName = databaseName,
                CreatedAt = createdAt,
                ValidationHash = validationHash,
                CreationMetadata = $"Created by TenantDatabaseValidator at {createdAt:O}",
            };

            context.TenantIdentity.Add(tenantIdentity);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Created tenant identity record for tenant {TenantId} in database {DatabaseName}",
                tenantId,
                databaseName);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error creating tenant identity for tenant {TenantId} in database {DatabaseName}",
                tenantId,
                databaseName);
            return Result.Fail($"Failed to create tenant identity: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<string>> GetValidatedConnectionStringAsync(Guid tenantId, string databaseName)
    {
        var validationResult = await ValidateTenantDatabaseAsync(tenantId, databaseName);

        if (validationResult.IsFailed)
        {
            return Result.Fail<string>(validationResult.Errors);
        }

        try
        {
            var connectionString = connectionStringProvider.GetTenantConnectionString(databaseName);
            return Result.Ok(connectionString);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error building connection string for tenant {TenantId} database {DatabaseName}",
                tenantId,
                databaseName);
            return Result.Fail<string>($"Failed to build connection string: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public void ClearValidationCache(Guid tenantId)
    {
        // We can't enumerate cache keys directly, so we use a pattern-based approach
        // In a production system, you might want to use a more sophisticated cache invalidation strategy
        _ = $"{CacheKeyPrefix}{tenantId}_";

        logger.LogInformation("Clearing validation cache for tenant {TenantId}", tenantId);

        // Note: MemoryCache doesn't provide a way to enumerate and remove by pattern
        // In a real application, consider using Redis or implementing cache key tracking
        // For now, we'll rely on natural expiration and provide this interface for future enhancement
    }

    /// <inheritdoc />
    public void ClearAllValidationCaches()
        // Note: MemoryCache doesn't provide a built-in way to clear by prefix
        // This would be better implemented with Redis or a custom cache wrapper
        => logger.LogInformation("Request to clear all validation caches (not fully implemented with MemoryCache)");

    private static string GetCacheKey(Guid tenantId, string databaseName) =>
        $"{CacheKeyPrefix}{tenantId}_{databaseName}";

    private async Task<Result> PerformValidationAsync(Guid tenantId, string databaseName)
    {
        try
        {
            // Get connection string for the tenant database
            string connectionString;
            try
            {
                connectionString = connectionStringProvider.GetTenantConnectionString(databaseName);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to get connection string for database {databaseName}: {ex.Message}");
            }

            // Connect to the tenant database and validate identity
            var options = new DbContextOptionsBuilder<TenantDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            await using var context = new TenantDbContext(options);

            // Try to find the tenant identity record
            var tenantIdentity = await context.TenantIdentity
                .FirstOrDefaultAsync(ti => ti.TenantId == tenantId);

            if (tenantIdentity == null)
            {
                return Result.Fail($"Tenant identity record not found for tenant {tenantId} in database {databaseName}");
            }

            // Validate database name matches
            if (tenantIdentity.DatabaseName != databaseName)
            {
                return Result.Fail($"Database name mismatch: identity record shows '{tenantIdentity.DatabaseName}' but accessing '{databaseName}'");
            }

            // Validate the hash to ensure the record hasn't been tampered with
            var expectedHash = GenerateValidationHash(tenantId, databaseName, tenantIdentity.CreatedAt);
            if (tenantIdentity.ValidationHash != expectedHash)
            {
                return Result.Fail($"Validation hash mismatch for tenant {tenantId} in database {databaseName}");
            }

            logger.LogDebug(
                "Successfully validated tenant {TenantId} ownership of database {DatabaseName}",
                tenantId,
                databaseName);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error during tenant database validation for tenant {TenantId} database {DatabaseName}",
                tenantId,
                databaseName);
            return Result.Fail($"Validation failed due to error: {ex.Message}");
        }
    }

    private string GenerateValidationHash(Guid tenantId, string databaseName, DateTime createdAt)
    {
        var validationSecret = configuration["TenantValidation:Secret"] ?? "default-secret-change-in-production";
        var input = $"{tenantId}|{databaseName}|{createdAt:O}|{validationSecret}";

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash);
    }
}
