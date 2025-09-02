namespace AtomicLmsCore.WebApi.Configuration.Options;

/// <summary>
/// Configuration options for health check security.
/// </summary>
public class HealthCheckSecurityOptions
{
    public const string SectionName = "HealthCheck";

    /// <summary>
    /// Gets or sets the secret header value required for detailed health checks.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the header name for health check authentication.
    /// </summary>
    public string HeaderName { get; set; } = "X-Health-Check-Key";
}
