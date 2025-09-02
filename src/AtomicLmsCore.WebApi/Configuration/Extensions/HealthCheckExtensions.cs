using System.Text.Json;
using AtomicLmsCore.Infrastructure.Persistence;
using AtomicLmsCore.WebApi.Configuration.Options;
using AtomicLmsCore.WebApi.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AtomicLmsCore.WebApi.Configuration.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure health check options
        services.Configure<HealthCheckSecurityOptions>(configuration.GetSection(HealthCheckSecurityOptions.SectionName));
        services.AddHealthChecks()
            .AddDbContextCheck<SolutionsDbContext>(
                "solutions-database",
                HealthStatus.Unhealthy,
                ["database", "critical"])
            .AddCheck<Auth0ConnectivityHealthCheck>(
                "auth0-connectivity",
                HealthStatus.Unhealthy,
                ["auth", "external", "critical"]);

        // Register HttpClient for Auth0 health check
        services.AddHttpClient<Auth0ConnectivityHealthCheck>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "AtomicLMS-HealthCheck/1.0");
        });

        return services;
    }

    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // Public endpoints (no authentication required)

        // Basic health check - just returns status (public)
        app.MapHealthChecks("/health");

        // Live check - basic connectivity for load balancers (public)
        app.MapHealthChecks(
            "/health/live",
            new HealthCheckOptions
            {
                Predicate = _ => false, // No checks, just returns 200 if app is running
            });

        // Protected endpoints (require custom header authentication)

        // Detailed health check with full JSON response (protected)
        app.MapHealthChecks(
            "/health/detailed",
            new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = report.Status.ToString(),
                        totalDuration = report.TotalDuration.TotalMilliseconds,
                        environment = app.Environment.EnvironmentName,
                        timestamp = DateTime.UtcNow,
                        checks = report.Entries.Select(entry => new
                        {
                            name = entry.Key,
                            status = entry.Value.Status.ToString(),
                            duration = entry.Value.Duration.TotalMilliseconds,
                            description = entry.Value.Description,
                            data = entry.Value.Data,
                            exception = entry.Value.Exception?.Message,
                            tags = entry.Value.Tags,
                        }),
                    };

                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(
                            response,
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                WriteIndented = true,
                            }));
                },
            });

        // Ready check - critical services with detailed timing (protected)
        app.MapHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("critical"),
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        status = report.Status.ToString(),
                        totalDuration = report.TotalDuration.TotalMilliseconds,
                        criticalServices = report.Entries
                            .Where(e => e.Value.Tags.Contains("critical"))
                            .Select(entry => new
                            {
                                name = entry.Key,
                                status = entry.Value.Status.ToString(),
                                duration = entry.Value.Duration.TotalMilliseconds,
                                description = entry.Value.Description,
                            }),
                    };

                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(
                            response,
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                WriteIndented = true,
                            }));
                },
            });

        return app;
    }
}
