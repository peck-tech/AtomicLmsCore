using FluentResults;

namespace AtomicLmsCore.Application.Common.Interfaces;

/// <summary>
///     Service for resolving the current user context based on the authentication type.
/// </summary>
public interface IUserContextService
{
    /// <summary>
    ///     Gets the authentication type of the current request.
    /// </summary>
    AuthenticationType AuthenticationType { get; }

    /// <summary>
    ///     Gets the authenticated subject (user or machine client).
    /// </summary>
    string? AuthenticatedSubject { get; }

    /// <summary>
    ///     Gets the target user ID for the current operation.
    ///     For user authentication: returns the authenticated user's ID.
    ///     For machine authentication: returns the on-behalf-of user's ID.
    /// </summary>
    string? TargetUserId { get; }

    /// <summary>
    ///     Determines if the current context is a machine-to-machine request.
    /// </summary>
    bool IsMachineToMachine { get; }

    /// <summary>
    ///     Validates that the current context has a valid target user.
    /// </summary>
    /// <returns>A result indicating success or failure with error details.</returns>
    Result ValidateUserContext();
}

/// <summary>
///     Represents the type of authentication used in the current request.
/// </summary>
public enum AuthenticationType
{
    /// <summary>
    ///     No authentication present.
    /// </summary>
    None,

    /// <summary>
    ///     User authenticated via Authorization Code flow with PKCE.
    /// </summary>
    User,

    /// <summary>
    ///     Machine authenticated via Client Credentials flow.
    /// </summary>
    Machine,
}
