using FluentResults;

namespace AtomicLmsCore.Application.Common.Interfaces;

/// <summary>
///     Service for checking permissions across both user and machine authentication flows.
///     Provides a unified permission model that works with both role-based (user) and scope-based (machine) authorization.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    ///     Checks if the current authenticated context has the specified permission.
    /// </summary>
    /// <param name="permission">The permission to check (e.g., "users:read", "tenants:manage").</param>
    /// <returns>True if the permission is granted, false otherwise.</returns>
    Task<bool> HasPermissionAsync(string permission);

    /// <summary>
    ///     Checks if the current authenticated context has any of the specified permissions.
    /// </summary>
    /// <param name="permissions">The permissions to check.</param>
    /// <returns>True if any of the permissions are granted, false otherwise.</returns>
    Task<bool> HasAnyPermissionAsync(params string[] permissions);

    /// <summary>
    ///     Checks if the current authenticated context has all of the specified permissions.
    /// </summary>
    /// <param name="permissions">The permissions to check.</param>
    /// <returns>True if all permissions are granted, false otherwise.</returns>
    Task<bool> HasAllPermissionsAsync(params string[] permissions);

    /// <summary>
    ///     Gets all permissions available to the current authenticated context.
    /// </summary>
    /// <returns>A list of all permissions granted to the current context.</returns>
    Task<IEnumerable<string>> GetPermissionsAsync();

    /// <summary>
    ///     Validates that the current context has the specified permission and returns a detailed result.
    /// </summary>
    /// <param name="permission">The permission to validate.</param>
    /// <returns>A result indicating success or failure with detailed error information.</returns>
    Task<Result> ValidatePermissionAsync(string permission);
}

/// <summary>
///     Common permission constants for the application.
/// </summary>
public static class Permissions
{
    /// <summary>
    ///     User management permissions.
    /// </summary>
    public static class Users
    {
        public const string Read = "users:read";
        public const string Create = "users:create";
        public const string Update = "users:update";
        public const string Delete = "users:delete";
        public const string Manage = "users:manage"; // Includes all user operations
    }

    /// <summary>
    ///     Tenant management permissions.
    /// </summary>
    public static class Tenants
    {
        public const string Read = "tenants:read";
        public const string Create = "tenants:create";
        public const string Update = "tenants:update";
        public const string Delete = "tenants:delete";
        public const string Manage = "tenants:manage"; // Includes all tenant operations
    }

    /// <summary>
    ///     Learning object permissions.
    /// </summary>
    public static class LearningObjects
    {
        public const string Read = "learning:read";
        public const string Create = "learning:create";
        public const string Update = "learning:update";
        public const string Delete = "learning:delete";
        public const string Manage = "learning:manage"; // Includes all learning operations
    }

    /// <summary>
    ///     System administration permissions.
    /// </summary>
    public static class System
    {
        public const string Admin = "system:admin"; // Full system access
    }
}
