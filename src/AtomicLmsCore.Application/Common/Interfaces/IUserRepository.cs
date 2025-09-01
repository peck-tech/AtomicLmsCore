using AtomicLmsCore.Domain.Entities;
using FluentResults;

namespace AtomicLmsCore.Application.Common.Interfaces;

/// <summary>
///     Repository interface for User entity operations in tenant-specific database.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    ///     Gets all users in the current tenant database.
    /// </summary>
    /// <returns>A result containing the list of users or errors.</returns>
    Task<Result<IEnumerable<User>>> GetAllAsync();

    /// <summary>
    ///     Gets a user by their public identifier.
    /// </summary>
    /// <param name="id">The user's public identifier.</param>
    /// <returns>A result containing the user or errors.</returns>
    Task<Result<User?>> GetByIdAsync(Guid id);

    /// <summary>
    ///     Gets a user by their external identity provider user ID.
    /// </summary>
    /// <param name="externalUserId">The external identity provider user identifier.</param>
    /// <returns>A result containing the user or errors.</returns>
    Task<Result<User?>> GetByExternalUserIdAsync(string externalUserId);

    /// <summary>
    ///     Gets a user by their email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>A result containing the user or errors.</returns>
    Task<Result<User?>> GetByEmailAsync(string email);

    /// <summary>
    ///     Adds a new user to the repository.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <returns>A result containing the added user or errors.</returns>
    Task<Result<User>> AddAsync(User user);

    /// <summary>
    ///     Updates an existing user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <returns>A result indicating success or errors.</returns>
    Task<Result> UpdateAsync(User user);

    /// <summary>
    ///     Soft deletes a user.
    /// </summary>
    /// <param name="id">The user's public identifier.</param>
    /// <returns>A result indicating success or errors.</returns>
    Task<Result> DeleteAsync(Guid id);

    /// <summary>
    ///     Checks if a user exists with the given email.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="excludeUserId">Optional user ID to exclude from the check.</param>
    /// <returns>A result containing whether the email exists or errors.</returns>
    Task<Result<bool>> EmailExistsAsync(string email, Guid? excludeUserId = null);

    /// <summary>
    ///     Checks if a user exists with the given external identity provider user ID.
    /// </summary>
    /// <param name="externalUserId">The external identity provider user identifier.</param>
    /// <returns>A result containing whether the external user ID exists or errors.</returns>
    Task<Result<bool>> ExternalUserIdExistsAsync(string externalUserId);
}
