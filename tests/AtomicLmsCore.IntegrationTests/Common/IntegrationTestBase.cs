using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AtomicLmsCore.IntegrationTests.Common;

public abstract class IntegrationTestBase(IntegrationTestWebApplicationFactory<Program> factory) : IClassFixture<IntegrationTestWebApplicationFactory<Program>>
{
    protected readonly HttpClient Client = factory.CreateClient();

    protected void SetTestUserRole(string role)
    {
        Client.DefaultRequestHeaders.Remove("X-Test-Role");
        Client.DefaultRequestHeaders.Add("X-Test-Role", role);
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
}
