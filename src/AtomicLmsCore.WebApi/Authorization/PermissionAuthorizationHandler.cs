using AtomicLmsCore.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace AtomicLmsCore.WebApi.Authorization;

/// <summary>
///     Authorization handler that validates permission requirements using the unified permission system.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="PermissionAuthorizationHandler"/> class.
/// </remarks>
/// <param name="permissionService">The permission service.</param>
/// <param name="logger">The logger instance.</param>
public class PermissionAuthorizationHandler(
    IPermissionService permissionService,
    ILogger<PermissionAuthorizationHandler> logger) : AuthorizationHandler<PermissionRequirement>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        try
        {
            var hasPermission = requirement.RequireAll ?
                await permissionService.HasAllPermissionsAsync(requirement.Permissions) :
                await permissionService.HasAnyPermissionAsync(requirement.Permissions);

            if (hasPermission)
            {
                logger.LogDebug(
                    "Permission requirement satisfied for {RequirementType}: [{Permissions}]",
                    requirement.RequireAll ? "ALL" : "ANY",
                    string.Join(", ", requirement.Permissions));

                context.Succeed(requirement);
            }
            else
            {
                logger.LogWarning(
                    "Permission requirement failed for {RequirementType}: [{Permissions}] - User lacks required permissions",
                    requirement.RequireAll ? "ALL" : "ANY",
                    string.Join(", ", requirement.Permissions));

                // Don't call context.Fail() - let other handlers run and return 403 by default
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error evaluating permission requirement for {RequirementType}: [{Permissions}]",
                requirement.RequireAll ? "ALL" : "ANY",
                string.Join(", ", requirement.Permissions));

            // Don't succeed on errors
        }
    }
}

/// <summary>
///     Represents a permission-based authorization requirement.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PermissionRequirement"/> class.
    /// </summary>
    /// <param name="permissions">The required permissions.</param>
    /// <param name="requireAll">Whether all permissions are required or any single permission is sufficient.</param>
    public PermissionRequirement(string[] permissions, bool requireAll = false)
    {
        Permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        RequireAll = requireAll;

        if (permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission must be specified", nameof(permissions));
        }
    }

    /// <summary>
    ///     Gets the required permissions.
    /// </summary>
    public string[] Permissions { get; }

    /// <summary>
    ///     Gets a value indicating whether all permissions are required (true) or any permission is sufficient (false).
    /// </summary>
    public bool RequireAll { get; }

    /// <summary>
    ///     Gets a string representation of this requirement.
    /// </summary>
    /// <returns>A string describing the requirement.</returns>
    public override string ToString()
    {
        var requirementType = RequireAll ? "ALL" : "ANY";
        return $"PermissionRequirement: {requirementType} of [{string.Join(", ", Permissions)}]";
    }
}
