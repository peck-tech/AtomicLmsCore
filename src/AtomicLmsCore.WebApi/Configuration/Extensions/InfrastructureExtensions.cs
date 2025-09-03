using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Tenants.Services;
using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Identity.Configuration;
using AtomicLmsCore.Infrastructure.Identity.Services;
using AtomicLmsCore.Infrastructure.Persistence.Services;
using AtomicLmsCore.Infrastructure.Services;

namespace AtomicLmsCore.WebApi.Configuration.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();
        services.AddMemoryCache();

        services
            .AddScoped<IIdGenerator, UlidIdGenerator>()
            .AddHttpContextAccessor()
            .AddScoped<ITenantAccessor, TenantAccessor>()
            .AddScoped<IConnectionStringProvider, ConnectionStringProvider>()
            .AddScoped<IDatabaseOperations, SqlServerDatabaseOperations>()
            .AddScoped<ITenantDatabaseValidator, TenantDatabaseValidator>()
            .AddScoped<ITenantDatabaseService, TenantDatabaseService>()
            .AddScoped<ITenantService, TenantService>();

        return services;
    }

    public static IServiceCollection AddIdentityProviderServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Auth0 as the identity provider implementation
        services.Configure<Auth0Options>(configuration.GetSection(Auth0Options.SectionName));
        services.AddScoped<IIdentityTokenService, Auth0TokenService>();
        services.AddScoped<IIdentityManagementService, Auth0ManagementService>();

        return services;
    }
}
