namespace AtomicLmsCore.Infrastructure.Identity.Configuration;

/// <summary>
///     Configuration options for Auth0 Management API.
/// </summary>
public class Auth0Options
{
    public const string SectionName = "Auth0";

    /// <summary>
    ///     Gets or sets the Auth0 domain.
    /// </summary>
    public required string Domain { get; set; }

    /// <summary>
    ///     Gets or sets the Auth0 client ID.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    ///     Gets or sets the Auth0 client secret.
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    ///     Gets or sets the Auth0 Management API audience.
    /// </summary>
    public required string ManagementApiAudience { get; set; }
}
