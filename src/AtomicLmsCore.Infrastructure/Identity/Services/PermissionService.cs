using System.Security.Claims;
using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Infrastructure.Identity.Services;

/// <summary>
///     Implementation of permission service that unifies role-based (user) and scope-based (machine) authorization.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="PermissionService"/> class.
/// </remarks>
/// <param name="httpContextAccessor">The HTTP context accessor.</param>
/// <param name="userContextService">The user context service.</param>
/// <param name="logger">The logger instance.</param>
public class PermissionService(
    IHttpContextAccessor httpContextAccessor,
    IUserContextService userContextService,
    ILogger<PermissionService> logger) : IPermissionService
{
    private const string ScopeClaimType = "scope";
    private const string PermissionClaimType = "permission";

    /// <summary>
    ///     Role to permission mappings for user authentication.
    /// </summary>
    private static readonly Dictionary<string, HashSet<string>> RolePermissionMappings = new()
    {
        {
            "superadmin", [
                Permissions.System.Admin,
                Permissions.Tenants.Manage,
                Permissions.Users.Manage,
                Permissions.LearningObjects.Manage
            ]
        },
        {
            "admin", [
                Permissions.Users.Manage,
                Permissions.LearningObjects.Manage
            ]
        },
        {
            "manager", [
                Permissions.Users.Read,
                Permissions.Users.Update,
                Permissions.LearningObjects.Manage
            ]
        },
        {
            "instructor", [
                Permissions.LearningObjects.Read,
                Permissions.LearningObjects.Create,
                Permissions.LearningObjects.Update,
                Permissions.Users.Read
            ]
        },
        {
            "learner", [
                Permissions.LearningObjects.Read
            ]
        },
    };

    /// <summary>
    ///     Permission hierarchy mappings (higher permissions include lower ones).
    /// </summary>
    private static readonly Dictionary<string, HashSet<string>> PermissionHierarchy = new()
    {
        {
            Permissions.Users.Manage, [
                Permissions.Users.Read,
                Permissions.Users.Create,
                Permissions.Users.Update,
                Permissions.Users.Delete
            ]
        },
        {
            Permissions.Tenants.Manage, [
                Permissions.Tenants.Read,
                Permissions.Tenants.Create,
                Permissions.Tenants.Update,
                Permissions.Tenants.Delete
            ]
        },
        {
            Permissions.LearningObjects.Manage, [
                Permissions.LearningObjects.Read,
                Permissions.LearningObjects.Create,
                Permissions.LearningObjects.Update,
                Permissions.LearningObjects.Delete
            ]
        },
    };

    /// <inheritdoc />
    public async Task<bool> HasPermissionAsync(string permission)
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var permissions = (await GetPermissionsAsync()).ToArray();
        var hasPermission = permissions.Contains(permission) || HasPermissionThroughHierarchy(permissions, permission);

        logger.LogDebug(
            hasPermission ? "Permission '{Permission}' granted to {AuthType} {Subject}" : "Permission '{Permission}' denied to {AuthType} {Subject}",
            permission,
            userContextService.IsMachineToMachine ? "machine" : "user",
            userContextService.AuthenticatedSubject);

        return hasPermission;
    }

    /// <inheritdoc />
    public async Task<bool> HasAnyPermissionAsync(params string[] permissions)
    {
        if (permissions.Length == 0)
        {
            return false;
        }

        foreach (var permission in permissions)
        {
            if (await HasPermissionAsync(permission))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> HasAllPermissionsAsync(params string[] permissions)
    {
        if (permissions.Length == 0)
        {
            return true;
        }

        foreach (var permission in permissions)
        {
            if (!await HasPermissionAsync(permission))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetPermissionsAsync()
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.User.Identity?.IsAuthenticated != true)
        {
            return [];
        }

        var permissions = new HashSet<string>();

        // Get permissions from direct permission claims (normalized during JWT processing)
        var permissionClaims = context.User.FindAll(PermissionClaimType);
        foreach (var claim in permissionClaims)
        {
            permissions.Add(claim.Value);
        }

        // Get permissions from scope claims (machine authentication)
        var scopeClaims = context.User.FindAll(ScopeClaimType);
        foreach (var claim in scopeClaims)
        {
            // Handle space-separated scopes in a single claim
            var scopes = claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var scope in scopes)
            {
                permissions.Add(scope);
            }
        }

        // Get permissions from roles (user authentication)
        var roleClaims = context.User.FindAll(ClaimTypes.Role);
        foreach (var roleClaim in roleClaims)
        {
            if (RolePermissionMappings.TryGetValue(roleClaim.Value, out var rolePermissions))
            {
                foreach (var permission in rolePermissions)
                {
                    permissions.Add(permission);
                }
            }
        }

        return permissions;
    }

    /// <inheritdoc />
    public async Task<Result> ValidatePermissionAsync(string permission)
    {
        var context = httpContextAccessor.HttpContext;
        if (context?.User.Identity?.IsAuthenticated != true)
        {
            return Result.Fail("Request is not authenticated");
        }

        var hasPermission = await HasPermissionAsync(permission);
        if (hasPermission)
        {
            return Result.Ok();
        }

        var authType = userContextService.IsMachineToMachine ? "Machine" : "User";
        var subject = userContextService.AuthenticatedSubject;
        var targetUser = userContextService.TargetUserId;

        var message = userContextService.IsMachineToMachine
            ? $"{authType} '{subject}' acting on behalf of user '{targetUser}' does not have permission '{permission}'"
            : $"{authType} '{subject}' does not have permission '{permission}'";

        return Result.Fail(message);
    }

    /// <summary>
    ///     Checks if any of the user's permissions grant access to the requested permission through hierarchy.
    /// </summary>
    /// <param name="userPermissions">The user's current permissions.</param>
    /// <param name="requestedPermission">The requested permission.</param>
    /// <returns>True if permission is granted through hierarchy, false otherwise.</returns>
    private static bool HasPermissionThroughHierarchy(IEnumerable<string> userPermissions, string requestedPermission)
    {
        foreach (var userPermission in userPermissions)
        {
            if (!PermissionHierarchy.TryGetValue(userPermission, out var impliedPermissions))
            {
                continue;
            }
            if (impliedPermissions.Contains(requestedPermission))
            {
                return true;
            }
        }

        return false;
    }
}
