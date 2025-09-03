using System.Security.Claims;
using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Infrastructure.Identity.Services;

/// <summary>
///     Implementation of user context service that handles both user and machine authentication.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="UserContextService"/> class.
/// </remarks>
/// <param name="httpContextAccessor">The HTTP context accessor.</param>
/// <param name="logger">The logger instance.</param>
public class UserContextService(
    IHttpContextAccessor httpContextAccessor,
    ILogger<UserContextService> logger) : IUserContextService
{
    private const string OnBehalfOfHeader = "X-On-Behalf-Of";
    private const string AuthenticationTypeKey = "AuthenticationType";
    private const string GrantTypeClaimType = "gty";
    private const string AzpClaimType = "azp";

    /// <inheritdoc />
    public AuthenticationType AuthenticationType
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context == null || !context.User.Identity?.IsAuthenticated == true)
            {
                return AuthenticationType.None;
            }

            // Check if we've already determined the authentication type
            if (context.Items.TryGetValue(AuthenticationTypeKey, out var cachedType) && cachedType is AuthenticationType type)
            {
                return type;
            }

            // Determine authentication type from claims
            var authenticationType = DetermineAuthenticationType(context.User);
            context.Items[AuthenticationTypeKey] = authenticationType;
            return authenticationType;
        }
    }

    /// <inheritdoc />
    public string? AuthenticatedSubject
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context?.User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // For machine auth, return the client ID (azp claim)
            if (AuthenticationType == AuthenticationType.Machine)
            {
                return context.User.FindFirst(AzpClaimType)?.Value ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            // For user auth, return the user ID (sub claim)
            return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }

    /// <inheritdoc />
    public string? TargetUserId
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context?.User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // For machine auth, get user from header
            if (AuthenticationType == AuthenticationType.Machine)
            {
                return context.Request.Headers.TryGetValue(OnBehalfOfHeader, out var userId) ? userId.ToString() : null;
            }

            // For user auth, return the authenticated user
            return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }

    /// <inheritdoc />
    public bool IsMachineToMachine => AuthenticationType == AuthenticationType.Machine;

    /// <inheritdoc />
    public Result ValidateUserContext()
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.User.Identity?.IsAuthenticated != true)
        {
            return Result.Fail("Request is not authenticated");
        }

        switch (AuthenticationType)
        {
            case AuthenticationType.None:
                return Result.Fail("Unable to determine authentication type");
            case AuthenticationType.Machine when string.IsNullOrEmpty(TargetUserId):
                return Result.Fail($"Machine-to-machine requests require {OnBehalfOfHeader} header to specify target user");
            case AuthenticationType.Machine:
                logger.LogInformation(
                    "Machine client {ClientId} acting on behalf of user {UserId}",
                    AuthenticatedSubject,
                    TargetUserId);
                break;
            case AuthenticationType.User:
            default:
                break;
        }

        return string.IsNullOrEmpty(TargetUserId) ?
            Result.Fail("Unable to determine target user for operation") :
            Result.Ok();
    }

    private AuthenticationType DetermineAuthenticationType(ClaimsPrincipal principal)
    {
        // Check for grant type claim (present in Client Credentials flow)
        var grantType = principal.FindFirst(GrantTypeClaimType)?.Value;
        if (grantType == "client-credentials")
        {
            logger.LogDebug("Detected Client Credentials authentication flow");
            return AuthenticationType.Machine;
        }

        // Check for sub claim (present in Authorization Code flow)
        var subClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst("sub")?.Value;

        // If we have a sub claim and no client-credentials grant type, it's user auth
        if (!string.IsNullOrEmpty(subClaim) && !subClaim.Contains("@clients", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogDebug("Detected Authorization Code authentication flow");
            return AuthenticationType.User;
        }

        // Check if sub claim indicates a machine client (Auth0 pattern)
        if (subClaim?.Contains("@clients", StringComparison.OrdinalIgnoreCase) == true)
        {
            logger.LogDebug("Detected machine client from sub claim pattern");
            return AuthenticationType.Machine;
        }

        // If we only have azp claim but no regular sub, it's likely machine auth
        var azpClaim = principal.FindFirst(AzpClaimType)?.Value;
        if (!string.IsNullOrEmpty(azpClaim) && string.IsNullOrEmpty(subClaim))
        {
            logger.LogDebug("Detected machine authentication from azp claim without sub");
            return AuthenticationType.Machine;
        }

        logger.LogWarning("Unable to determine authentication type from claims");
        return AuthenticationType.None;
    }
}
