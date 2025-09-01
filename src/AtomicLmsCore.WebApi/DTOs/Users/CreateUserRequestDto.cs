using System.ComponentModel.DataAnnotations;

namespace AtomicLmsCore.WebApi.DTOs.Users;

/// <summary>
///     DTO for creating a new user.
/// </summary>
public class CreateUserRequestDto
{
    /// <summary>
    ///     The external identity provider user ID for authentication.
    /// </summary>
    [Required]
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>
    ///     The user's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     The user's first name.
    /// </summary>
    [Required]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    ///     The user's last name.
    /// </summary>
    [Required]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    ///     The user's display name.
    /// </summary>
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the user should be active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Additional metadata for the user.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; set; }
}
