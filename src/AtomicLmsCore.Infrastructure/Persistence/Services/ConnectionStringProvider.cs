using AtomicLmsCore.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AtomicLmsCore.Infrastructure.Persistence.Services;

/// <summary>
///     Provides connection strings for the multi-database architecture.
/// </summary>
public class ConnectionStringProvider(IConfiguration configuration) : IConnectionStringProvider
{
    /// <inheritdoc />
    public string GetSolutionsConnectionString()
    {
        var connectionString = configuration.GetConnectionString("SolutionsDatabase");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Solutions database connection string not configured");
        }

        return connectionString;
    }

    /// <inheritdoc />
    public string GetTenantConnectionString(string databaseName)
    {
        var template = configuration.GetConnectionString("TenantDatabaseTemplate");
        if (string.IsNullOrEmpty(template))
        {
            throw new InvalidOperationException("Tenant database connection string template not configured");
        }

        // Replace the placeholder with the actual database name
        return template.Replace("{DatabaseName}", databaseName, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public async Task<bool> TenantDatabaseExistsAsync(string databaseName)
    {
        var masterConnectionString = configuration.GetConnectionString("MasterDatabase");

        if (string.IsNullOrEmpty(masterConnectionString))
        {
            // If no master connection string, assume database exists
            return true;
        }

        try
        {
            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName";
            command.Parameters.AddWithValue("@DatabaseName", databaseName);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        catch
        {
            // In case of any error, assume database exists to avoid blocking operations
            return true;
        }
    }
}
