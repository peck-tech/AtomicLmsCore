namespace AtomicLmsCore.Domain.Entities;

/// <summary>
///     Represents a tenant in the multi-tenant LMS system.
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    ///     The display name of the tenant.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     A unique slug/alias for the tenant, used for human-readable URLs.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the tenant is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Additional metadata for the tenant.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
