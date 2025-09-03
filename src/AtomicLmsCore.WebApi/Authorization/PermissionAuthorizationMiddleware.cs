using System.Text.Json;
using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.WebApi.Common;

namespace AtomicLmsCore.WebApi.Authorization;

/// <summary>
///     Middleware that processes RequirePermissionAttribute and validates permissions.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="PermissionAuthorizationMiddleware"/> class.
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="logger">The logger instance.</param>
public class PermissionAuthorizationMiddleware(
    RequestDelegate next,
    ILogger<PermissionAuthorizationMiddleware> logger)
{
    /// <summary>
    ///     Processes the HTTP context to validate permission requirements.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="permissionService">The permission service.</param>
    /// <param name="userContextService">The user context service.</param>
    public async Task InvokeAsync(
        HttpContext context,
        IPermissionService permissionService,
        IUserContextService userContextService)
    {
        // Skip if not authenticated (will be handled by [Authorize])
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await next(context);
            return;
        }

        // Get the endpoint and check for permission attributes
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await next(context);
            return;
        }

        var permissionAttributes = endpoint.Metadata.GetOrderedMetadata<RequirePermissionAttribute>();
        if (!permissionAttributes.Any())
        {
            // No permission attributes, continue to next middleware
            await next(context);
            return;
        }

        // Validate all permission requirements
        foreach (var attribute in permissionAttributes)
        {
            bool hasPermission;

            if (attribute.RequireAll)
            {
                hasPermission = await permissionService.HasAllPermissionsAsync(attribute.Permissions);
            }
            else
            {
                hasPermission = await permissionService.HasAnyPermissionAsync(attribute.Permissions);
            }

            if (!hasPermission)
            {
                // Log the authorization failure
                var requirementType = attribute.RequireAll ? "ALL" : "ANY";
                var authType = userContextService.IsMachineToMachine ? "Machine" : "User";
                var subject = userContextService.AuthenticatedSubject;

                logger.LogWarning(
                    "Authorization failed: {AuthType} '{Subject}' lacks required permissions. " +
                    "Required: {RequirementType} of [{Permissions}]",
                    authType,
                    subject,
                    requirementType,
                    string.Join(", ", attribute.Permissions));

                // Return 403 Forbidden
                await WriteForbiddenResponseAsync(
                    context,
                    $"Insufficient permissions. Required: {requirementType} of [{string.Join(", ", attribute.Permissions)}]");
                return;
            }
        }

        // All permission requirements satisfied
        logger.LogDebug(
            "Permission authorization succeeded for {AuthType} '{Subject}'",
            userContextService.IsMachineToMachine ? "Machine" : "User",
            userContextService.AuthenticatedSubject);

        await next(context);
    }

    /// <summary>
    ///     Writes a 403 Forbidden response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="message">The error message.</param>
    private static async Task WriteForbiddenResponseAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";

        var errorResponse = ErrorResponseDto.ForbiddenError(
            message,
            context.Items["CorrelationId"]?.ToString());

        var json = JsonSerializer.Serialize(
            errorResponse,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

        await context.Response.WriteAsync(json);
    }
}
