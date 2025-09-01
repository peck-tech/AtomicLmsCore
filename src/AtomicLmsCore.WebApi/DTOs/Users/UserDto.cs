namespace AtomicLmsCore.WebApi.DTOs.Users;

/// <summary>
///     DTO for user information with metadata.
/// </summary>
public class UserDto
{
    /// <summary>
    ///     The unique identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

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
    ///     The tenant ID this user belongs to.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    ///     Indicates whether the user is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Additional metadata for the user.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    /// <summary>
    ///     The date and time when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     The date and time when the user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
