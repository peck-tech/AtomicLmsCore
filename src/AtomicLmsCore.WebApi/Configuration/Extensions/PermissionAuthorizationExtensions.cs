using AtomicLmsCore.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace AtomicLmsCore.WebApi.Configuration.Extensions;

/// <summary>
///     Extension methods for configuring permission-based authorization.
/// </summary>
public static class PermissionAuthorizationExtensions
{
    /// <summary>
    ///     Adds permission-based authorization to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        // Register the permission authorization handler
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Register dynamic permission authorization handler
        services.AddScoped<IAuthorizationHandler, DynamicPermissionAuthorizationHandler>();

        // Register custom policy provider for dynamic permission policies
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Register middleware for processing permission attributes
        services.AddScoped<PermissionAuthorizationMiddleware>();

        return services;
    }
}
