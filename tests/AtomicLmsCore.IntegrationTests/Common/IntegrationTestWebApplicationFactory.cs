using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtomicLmsCore.IntegrationTests.Common;

public class IntegrationTestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Clear existing configuration sources and add test configuration
            config.Sources.Clear();
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:SolutionsDatabase"] = "Data Source=:memory:",
                ["ConnectionStrings:TenantDatabaseTemplate"] = "Data Source=:memory:",
                ["ConnectionStrings:MasterDatabase"] = "Data Source=:memory:",
                ["Jwt:SecretKey"] = "TestSecretKeyThatIsLongEnoughForTesting123456789",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing database context registrations
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(SolutionsDbContext) ||
                d.ServiceType == typeof(TenantDbContext) ||
                d.ServiceType == typeof(DbContextOptions<SolutionsDbContext>) ||
                d.ServiceType == typeof(DbContextOptions<TenantDbContext>)).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Register in-memory database contexts for testing
            services.AddScoped(_ =>
            {
                var options = new DbContextOptionsBuilder<SolutionsDbContext>()
                    .UseInMemoryDatabase("TestSolutionsDb")
                    .Options;
                var dbContext = new SolutionsDbContext(options);
                dbContext.Database.EnsureCreated();
                return dbContext;
            });

            services.AddScoped(serviceProvider =>
            {
                var idGenerator = serviceProvider.GetRequiredService<Domain.Services.IIdGenerator>();
                var options = new DbContextOptionsBuilder<TenantDbContext>()
                    .UseInMemoryDatabase("TestTenantDb")
                    .Options;
                var dbContext = new TenantDbContext(options, idGenerator);
                dbContext.Database.EnsureCreated();
                return dbContext;
            });

            // Replace ITenantAccessor with test implementation
            var tenantAccessorDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITenantAccessor));
            if (tenantAccessorDescriptor != null)
            {
                services.Remove(tenantAccessorDescriptor);
            }
            services.AddScoped<ITenantAccessor, TestTenantAccessor>();

            // Replace authentication with test authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "Test", _ => { });
        });

        builder.UseEnvironment("Test");
    }

}
