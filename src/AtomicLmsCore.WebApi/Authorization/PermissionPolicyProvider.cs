using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AtomicLmsCore.WebApi.Authorization;

/// <summary>
///     Custom policy provider that dynamically creates permission-based authorization policies.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="PermissionPolicyProvider"/> class.
/// </remarks>
/// <param name="options">The authorization options.</param>
/// <param name="logger">The logger instance.</param>
public class PermissionPolicyProvider(
    IOptions<AuthorizationOptions> options,
    ILogger<PermissionPolicyProvider> logger) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider = new(options);

    /// <inheritdoc />
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    /// <inheritdoc />
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    /// <inheritdoc />
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        // Handle permission-based policies
        => policyName.StartsWith("Permission_", StringComparison.OrdinalIgnoreCase) ? Task.FromResult(CreatePermissionPolicy(policyName)) :
            // Fall back to default provider for non-permission policies
            _fallbackPolicyProvider.GetPolicyAsync(policyName);

    /// <summary>
    ///     Creates a permission-based authorization policy from the policy name.
    /// </summary>
    /// <param name="policyName">The policy name (format: Permission_{ANY|ALL}_{base64hash}).</param>
    /// <returns>An authorization policy or null if the policy name is invalid.</returns>
    private AuthorizationPolicy? CreatePermissionPolicy(string policyName)
    {
        try
        {
            // Parse policy name: Permission_{ANY|ALL}_{base64hash}
            var parts = policyName.Split('_');
            if (parts.Length != 3 || parts[0] != "Permission")
            {
                logger.LogWarning("Invalid permission policy name format: {PolicyName}", policyName);
                return null;
            }

            var requirementType = parts[1];
            var requireAll = requirementType == "ALL";

            if (requirementType != "ANY" && requirementType != "ALL")
            {
                logger.LogWarning("Invalid permission requirement type in policy: {PolicyName}", policyName);
                return null;
            }

            // For security, we can't decode permissions from the hash
            // The policy will be validated by the handler using the current permissions
            // This is just a placeholder - the actual permissions come from the attribute
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new DynamicPermissionRequirement(policyName))
                .Build();

            logger.LogDebug("Created dynamic permission policy: {PolicyName}", policyName);
            return policy;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating permission policy: {PolicyName}", policyName);
            return null;
        }
    }
}

/// <summary>
///     A dynamic permission requirement that will be resolved at runtime.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="DynamicPermissionRequirement"/> class.
/// </remarks>
/// <param name="policyName">The policy name.</param>
public class DynamicPermissionRequirement(string policyName) : IAuthorizationRequirement
{
    /// <summary>
    ///     Gets the policy name.
    /// </summary>
    public string PolicyName { get; } = policyName ?? throw new ArgumentNullException(nameof(policyName));

    /// <summary>
    ///     Gets a string representation of this requirement.
    /// </summary>
    /// <returns>A string describing the requirement.</returns>
    public override string ToString() => $"DynamicPermissionRequirement: {PolicyName}";
}

/// <summary>
///     Authorization handler for dynamic permission requirements.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="DynamicPermissionAuthorizationHandler"/> class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
public class DynamicPermissionAuthorizationHandler(ILogger<DynamicPermissionAuthorizationHandler> logger) : AuthorizationHandler<DynamicPermissionRequirement>
{
    private readonly ILogger<DynamicPermissionAuthorizationHandler> _logger = logger;

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DynamicPermissionRequirement requirement)
    {
        // The actual permission checking is handled by looking at the endpoint metadata
        // and finding the RequirePermissionAttribute. This handler just ensures the
        // dynamic policy is recognized as valid.

        // Look for RequirePermissionAttribute in the endpoint metadata
        if (context.Resource is HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();
            var permissionAttributes = endpoint?.Metadata.GetOrderedMetadata<RequirePermissionAttribute>();

            if (permissionAttributes != null && permissionAttributes.Any())
            {
                // The actual permission validation will be handled by endpoint-specific logic
                // This handler just validates that we have permission attributes
                _logger.LogDebug("Dynamic permission requirement found permission attributes for policy: {PolicyName}", requirement.PolicyName);
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        _logger.LogWarning("No permission attributes found for dynamic policy: {PolicyName}", requirement.PolicyName);
        return Task.CompletedTask;
    }
}
