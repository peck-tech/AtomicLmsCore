using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtomicLmsCore.Infrastructure.Persistence.Design;

/// <summary>
///     Design-time factory for SolutionsDbContext.
///     Used by Entity Framework migrations tool.
/// </summary>
public class SolutionsDbContextFactory : IDesignTimeDbContextFactory<SolutionsDbContext>
{
    public SolutionsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SolutionsDbContext>();

        // Use a default SQL Server connection string for design-time operations
        optionsBuilder.UseSqlServer("Server=localhost;Database=AtomicLms_Solutions;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true");

        return new SolutionsDbContext(optionsBuilder.Options);
    }
}
