using FluentResults;

namespace AtomicLmsCore.Application.Common.Interfaces;

/// <summary>
///     Service for managing users in an external identity provider.
/// </summary>
public interface IIdentityManagementService
{
    /// <summary>
    ///     Gets a user by their identity provider user ID.
    /// </summary>
    /// <param name="userId">The identity provider user ID.</param>
    /// <returns>A result containing the user identity or error information.</returns>
    Task<Result<IdentityUser>> GetUserAsync(string userId);

    /// <summary>
    ///     Creates a new user in the identity provider.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>A result containing the created user identity or error information.</returns>
    Task<Result<IdentityUser>> CreateUserAsync(string email, string password);

    /// <summary>
    ///     Updates a user's metadata in the identity provider.
    /// </summary>
    /// <param name="userId">The identity provider user ID.</param>
    /// <param name="metadata">The metadata to update.</param>
    /// <returns>A result containing the updated user identity or error information.</returns>
    Task<Result<IdentityUser>> UpdateUserMetadataAsync(string userId, Dictionary<string, object> metadata);

    /// <summary>
    ///     Assigns a role to a user.
    /// </summary>
    /// <param name="userId">The identity provider user ID.</param>
    /// <param name="roleId">The role ID.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> AssignRoleToUserAsync(string userId, string roleId);

    /// <summary>
    ///     Gets all roles for a user.
    /// </summary>
    /// <param name="userId">The identity provider user ID.</param>
    /// <returns>A result containing the user's roles or error information.</returns>
    Task<Result<IList<IdentityRole>>> GetUserRolesAsync(string userId);
}

/// <summary>
///     Represents a user in the identity provider.
/// </summary>
public class IdentityUser
{
    /// <summary>
    ///     Gets or sets the user ID in the identity provider.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets whether the user's email is verified.
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    ///     Gets or sets the user's metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
///     Represents a role in the identity provider.
/// </summary>
public class IdentityRole
{
    /// <summary>
    ///     Gets or sets the role ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the role name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the role description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
