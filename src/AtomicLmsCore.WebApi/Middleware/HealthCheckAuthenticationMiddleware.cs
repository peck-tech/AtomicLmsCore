using AtomicLmsCore.WebApi.Configuration.Options;
using Microsoft.Extensions.Options;

namespace AtomicLmsCore.WebApi.Middleware;

/// <summary>
/// Middleware to authenticate detailed health check requests using a custom header.
/// </summary>
public class HealthCheckAuthenticationMiddleware(
    RequestDelegate next,
    IOptions<HealthCheckSecurityOptions> options,
    ILogger<HealthCheckAuthenticationMiddleware> logger)
{
    private readonly HealthCheckSecurityOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();

        // Only protect detailed health check endpoints
        if (path is "/health/detailed" or "/health/ready")
        {
            if (string.IsNullOrEmpty(_options.Secret))
            {
                logger.LogWarning("Health check secret not configured - blocking detailed health checks");
                context.Response.StatusCode = 503;
                await context.Response.WriteAsync("Health check authentication not configured");
                return;
            }

            var headerValue = context.Request.Headers[_options.HeaderName].FirstOrDefault();

            if (string.IsNullOrEmpty(headerValue))
            {
                logger.LogWarning(
                    "Health check request to {Path} missing authentication header {HeaderName}",
                    path,
                    _options.HeaderName);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Authentication header required");
                return;
            }

            if (!string.Equals(headerValue, _options.Secret, StringComparison.Ordinal))
            {
                logger.LogWarning(
                    "Health check request to {Path} with invalid authentication header from {RemoteIP}",
                    path,
                    context.Connection.RemoteIpAddress);
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Invalid authentication");
                return;
            }

            logger.LogDebug("Health check request to {Path} authenticated successfully", path);
        }

        await next(context);
    }
}
