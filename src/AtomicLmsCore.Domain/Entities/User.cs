namespace AtomicLmsCore.Domain.Entities;

/// <summary>
///     Represents a user in the LMS system.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    ///     The external identity provider user ID for authentication.
    /// </summary>
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>
    ///     The user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     The user's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    ///     The user's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    ///     The user's display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     The tenant this user belongs to.
    /// </summary>
    public int TenantInternalId { get; set; }

    /// <summary>
    ///     Reference to the tenant entity.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    ///     Indicates whether the user is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Additional metadata for the user.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
