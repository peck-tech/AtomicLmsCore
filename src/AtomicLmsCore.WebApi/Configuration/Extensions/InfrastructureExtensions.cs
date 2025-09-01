using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Tenants.Services;
using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Services;

namespace AtomicLmsCore.WebApi.Configuration.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();

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
}
