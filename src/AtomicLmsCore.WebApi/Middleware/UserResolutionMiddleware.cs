using System.Text.Json;
using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.WebApi.Common;

namespace AtomicLmsCore.WebApi.Middleware;

/// <summary>
///     Middleware for resolving and validating user context based on authentication type.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="UserResolutionMiddleware"/> class.
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="logger">The logger instance.</param>
public class UserResolutionMiddleware(
    RequestDelegate next,
    ILogger<UserResolutionMiddleware> logger)
{
    /// <summary>
    ///     Processes the HTTP context to resolve and validate user context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="userContextService">The user context service.</param>
    public async Task InvokeAsync(HttpContext context, IUserContextService userContextService)
    {
        // Skip if not an API request
        if (!IsApiRequest(context.Request.Path))
        {
            await next(context);
            return;
        }

        // Skip if user is not authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await next(context);
            return;
        }

        // Validate user context for authenticated requests
        var validationResult = userContextService.ValidateUserContext();
        if (!validationResult.IsSuccess)
        {
            logger.LogWarning(
                "User context validation failed: {Errors}",
                string.Join(", ", validationResult.Errors.Select(e => e.Message)));

            await WriteBadRequestResponseAsync(
                context,
                validationResult.Errors.FirstOrDefault()?.Message ?? "Invalid user context");
            return;
        }

        // Log the resolved context
        if (userContextService.IsMachineToMachine)
        {
            logger.LogInformation(
                "Machine-to-machine request: Client {ClientId} acting as user {UserId}",
                userContextService.AuthenticatedSubject,
                userContextService.TargetUserId);
        }
        else
        {
            logger.LogDebug(
                "User request: User {UserId}",
                userContextService.TargetUserId);
        }

        // Store resolved user ID in context for downstream use
        context.Items["ResolvedUserId"] = userContextService.TargetUserId;
        context.Items["IsMachineToMachine"] = userContextService.IsMachineToMachine;

        await next(context);
    }

    private static bool IsApiRequest(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;
        return pathValue.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task WriteBadRequestResponseAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var errorResponse = ErrorResponseDto.BadRequestError(
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
