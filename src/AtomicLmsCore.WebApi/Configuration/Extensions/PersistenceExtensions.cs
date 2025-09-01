using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Infrastructure.Persistence;
using AtomicLmsCore.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AtomicLmsCore.WebApi.Configuration.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped(serviceProvider =>
        {
            var connectionStringProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
            var connectionString = connectionStringProvider.GetSolutionsConnectionString();

            var options = new DbContextOptionsBuilder<SolutionsDbContext>()
                .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(SolutionsDbContext).Assembly.FullName))
                .Options;

            return new SolutionsDbContext(options);
        });

        services.AddScoped<ITenantRepository, TenantRepository>();

        services.AddScoped(serviceProvider =>
        {
            var tenantAccessor = serviceProvider.GetRequiredService<ITenantAccessor>();
            var connectionStringProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
            var tenantRepository = serviceProvider.GetRequiredService<ITenantRepository>();
            var idGenerator = serviceProvider.GetRequiredService<Domain.Services.IIdGenerator>();

            var currentTenantId = tenantAccessor.GetRequiredCurrentTenantId();

            var tenant = tenantRepository.GetByIdAsync(currentTenantId).GetAwaiter().GetResult();
            if (tenant == null || string.IsNullOrEmpty(tenant.DatabaseName))
            {
                throw new InvalidOperationException($"Tenant {currentTenantId} not found or missing database name");
            }

            var connectionString = connectionStringProvider.GetTenantConnectionString(tenant.DatabaseName);
            var options = new DbContextOptionsBuilder<TenantDbContext>()
                .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(TenantDbContext).Assembly.FullName))
                .Options;

            return new TenantDbContext(options, idGenerator);
        });

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
