using Microsoft.AspNetCore.Authorization;

namespace AtomicLmsCore.WebApi.Authorization;

/// <summary>
///     Authorization attribute that requires one or more permissions.
///     Works with both user roles and machine scopes through the unified permission system.
/// </summary>
/// <remarks>
///     Examples:
///     [RequirePermission("users:read")] - Requires single permission
///     [RequirePermission("users:read", "users:write")] - Requires any of the permissions
///     [RequirePermission("users:manage", RequireAll = true)] - For single permission (default).
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RequirePermissionAttribute"/> class.
    /// </summary>
    /// <param name="permission">The required permission.</param>
    public RequirePermissionAttribute(string permission)
        : this([permission])
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequirePermissionAttribute"/> class.
    /// </summary>
    /// <param name="permissions">The required permissions.</param>
    public RequirePermissionAttribute(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
        {
            throw new ArgumentException("At least one permission must be specified", nameof(permissions));
        }

        Permissions = permissions;
        Policy = CreatePolicyName(permissions, false);
    }

    /// <summary>
    ///     Gets the required permissions.
    /// </summary>
    public string[] Permissions { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether all permissions are required (true) or any permission is sufficient (false).
    ///     Default is false (any permission is sufficient).
    /// </summary>
    public bool RequireAll { get; set; }

    /// <summary>
    ///     Gets or sets the policy name. This is automatically generated based on permissions and RequireAll setting.
    /// </summary>
    public new string Policy
    {
        get => base.Policy ?? string.Empty;
        set => base.Policy = value;
    }

    /// <summary>
    ///     Updates the policy name when RequireAll changes.
    /// </summary>
    public void UpdatePolicy()
        => Policy = CreatePolicyName(Permissions, RequireAll);

    /// <summary>
    ///     Creates a unique policy name for the given permissions and requirement type.
    /// </summary>
    /// <param name="permissions">The permissions.</param>
    /// <param name="requireAll">Whether all permissions are required.</param>
    /// <returns>A unique policy name.</returns>
    private static string CreatePolicyName(string[] permissions, bool requireAll)
    {
        var sortedPermissions = permissions.OrderBy(p => p).ToArray();
        var permissionsHash = string.Join("|", sortedPermissions);
        var requirementType = requireAll ? "ALL" : "ANY";
        return $"Permission_{requirementType}_{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(permissionsHash)).Replace("=", string.Empty, StringComparison.InvariantCulture).Replace("+", "-", StringComparison.InvariantCulture).Replace("/", "_", StringComparison.InvariantCulture)}";
    }
}
