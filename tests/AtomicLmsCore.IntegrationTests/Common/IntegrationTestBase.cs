using AtomicLmsCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AtomicLmsCore.IntegrationTests.Common;

public abstract class IntegrationTestBase(IntegrationTestWebApplicationFactory<Program> factory) : IClassFixture<IntegrationTestWebApplicationFactory<Program>>, IAsyncLifetime
{
    protected readonly HttpClient Client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        // Clean databases before each test
        await CleanDatabase<SolutionsDbContext>();
        await CleanDatabase<TenantDbContext>();

        // Set default authentication header (tests can override this)
        SetTestAuthentication();
    }

    public Task DisposeAsync()
        // Clean up after each test
        => Task.CompletedTask;

    protected void SetTestUserRole(string role)
    {
        Client.DefaultRequestHeaders.Remove("X-Test-Role");
        Client.DefaultRequestHeaders.Add("X-Test-Role", role);
    }

    protected void SetTestUserPermissions(params string[] permissions)
    {
        Client.DefaultRequestHeaders.Remove("X-Test-Permissions");
        Client.DefaultRequestHeaders.Add("X-Test-Permissions", string.Join(",", permissions));
    }

    protected void SetTestUserTenant(Guid tenantId)
    {
        Client.DefaultRequestHeaders.Remove("X-Test-Tenant");
        Client.DefaultRequestHeaders.Add("X-Test-Tenant", tenantId.ToString());
    }

    protected void SetTenantHeader(Guid tenantId)
    {
        Client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        Client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
    }

    private void SetTestAuthentication()
    {
        Client.DefaultRequestHeaders.Remove("X-Test-Auth");
        Client.DefaultRequestHeaders.Add("X-Test-Auth", "true");
    }

    protected T GetDbContext<T>() where T : DbContext
    {
        var scope = factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    protected async Task SeedDatabase<T>(Action<T> seedAction) where T : DbContext
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<T>();
        seedAction(context);
        await context.SaveChangesAsync();
    }

    private async Task CleanDatabase<T>() where T : DbContext
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<T>();

        // For in-memory databases, we can recreate the database to ensure clean state
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
}
