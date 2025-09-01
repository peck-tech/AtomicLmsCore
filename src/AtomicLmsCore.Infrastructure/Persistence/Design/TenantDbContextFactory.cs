using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtomicLmsCore.Infrastructure.Persistence.Design;

/// <summary>
///     Design-time factory for TenantDbContext.
///     Used by Entity Framework migrations tool.
/// </summary>
public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();

        // Use a default SQL Server connection string for design-time operations
        optionsBuilder.UseSqlServer("Server=localhost;Database=AtomicLms_DefaultTenant;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true");

        // Create a default ID generator
        IIdGenerator idGenerator = new UlidIdGenerator();

        return new TenantDbContext(optionsBuilder.Options, idGenerator);
    }
}
