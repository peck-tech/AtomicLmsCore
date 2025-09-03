using FluentResults;

namespace AtomicLmsCore.Application.Common.Interfaces;

/// <summary>
///     Service for managing identity provider access tokens.
/// </summary>
public interface IIdentityTokenService
{
    /// <summary>
    ///     Gets an access token for the identity provider's management API.
    /// </summary>
    /// <returns>A result containing the access token or error information.</returns>
    Task<Result<string>> GetManagementTokenAsync();

    /// <summary>
    ///     Gets an access token for a specific audience.
    /// </summary>
    /// <param name="audience">The audience for the token.</param>
    /// <returns>A result containing the access token or error information.</returns>
    Task<Result<string>> GetTokenAsync(string audience);
}
