namespace AtomicLmsCore.WebApi.Configuration;

/// <summary>
///     Configuration options for JWT authentication with Auth0.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    ///     Gets or sets the Auth0 domain (authority).
    /// </summary>
    required public string Authority { get; set; }

    /// <summary>
    ///     Gets or sets the audience identifier for the API.
    /// </summary>
    required public string Audience { get; set; }

    /// <summary>
    ///     Gets or sets whether HTTPS is required for metadata address (default: true).
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the token audience should be validated (default: true).
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the token issuer should be validated (default: true).
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether the token lifetime should be validated (default: true).
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;
}
