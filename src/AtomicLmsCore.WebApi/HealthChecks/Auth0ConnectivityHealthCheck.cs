using System.Text.Json;
using AtomicLmsCore.WebApi.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace AtomicLmsCore.WebApi.HealthChecks;

public class Auth0ConnectivityHealthCheck(
    HttpClient httpClient,
    IOptions<JwtOptions> jwtOptions,
    ILogger<Auth0ConnectivityHealthCheck> logger) : IHealthCheck
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_jwtOptions.Authority))
            {
                return HealthCheckResult.Unhealthy("JWT Authority is not configured");
            }

            var wellKnownUrl = $"{_jwtOptions.Authority.TrimEnd('/')}/.well-known/openid-configuration";

            using var response = await httpClient.GetAsync(wellKnownUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy(
                    $"Auth0 OpenID Configuration endpoint returned {response.StatusCode}",
                    data: new Dictionary<string, object>
                    {
                        ["endpoint"] = wellKnownUrl,
                        ["status_code"] = response.StatusCode,
                    });
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var jsonDocument = JsonDocument.Parse(content);

            var hasIssuer = jsonDocument.RootElement.TryGetProperty("issuer", out _);
            var hasJwksUri = jsonDocument.RootElement.TryGetProperty("jwks_uri", out _);

            if (!hasIssuer || !hasJwksUri)
            {
                return HealthCheckResult.Degraded(
                    "Auth0 configuration is missing required properties",
                    data: new Dictionary<string, object>
                    {
                        ["has_issuer"] = hasIssuer,
                        ["has_jwks_uri"] = hasJwksUri,
                    });
            }

            return HealthCheckResult.Healthy(
                "Auth0 connectivity verified",
                data: new Dictionary<string, object>
                {
                    ["endpoint"] = wellKnownUrl,
                    ["response_time_ms"] = response.Headers.Date?.Subtract(DateTime.UtcNow).TotalMilliseconds ?? 0,
                });
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Auth0 connectivity health check failed - network error");
            return HealthCheckResult.Unhealthy(
                "Failed to connect to Auth0",
                ex,
                data: new Dictionary<string, object>
                {
                    ["error_type"] = "network_error",
                });
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "Auth0 connectivity health check timed out");
            return HealthCheckResult.Unhealthy(
                "Auth0 connectivity check timed out",
                ex,
                data: new Dictionary<string, object>
                {
                    ["error_type"] = "timeout",
                });
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Auth0 returned invalid JSON response");
            return HealthCheckResult.Degraded(
                "Auth0 returned invalid configuration",
                ex,
                data: new Dictionary<string, object>
                {
                    ["error_type"] = "invalid_json",
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during Auth0 health check");
            return HealthCheckResult.Unhealthy(
                "Unexpected error during Auth0 health check",
                ex);
        }
    }
}
