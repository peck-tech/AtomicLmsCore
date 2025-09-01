using System.Security.Claims;
using System.Text.Json;
using AtomicLmsCore.WebApi.Common;

namespace AtomicLmsCore.WebApi.Middleware;

public class TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
{
    private const string TenantIdHeader = "X-Tenant-Id";
    private const string TenantClaimType = "tenant_id";
    private const string SuperAdminRole = "superadmin";
    private const string ValidatedTenantIdKey = "ValidatedTenantId";

    private static Guid? GetTenantIdFromHeader(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TenantIdHeader, out var tenantIdHeader) &&
            Guid.TryParse(tenantIdHeader, out var tenantId) &&
            tenantId != Guid.Empty)
        {
            return tenantId;
        }
        return null;
    }

    private static List<Guid> GetTenantIdsFromClaims(ClaimsPrincipal user) =>
        user.FindAll(TenantClaimType)
            .Select(c => c.Value)
            .Where(v => Guid.TryParse(v, out _))
            .Select(v => Guid.Parse(v))
            .Distinct()
            .ToList();

    private static bool IsSolutionFeatureBucketRequest(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;

        if (!pathValue.StartsWith("/api/v", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var versionEndIndex = pathValue.IndexOf('/', 6);
        if (versionEndIndex == -1)
        {
            return false;
        }

        var remainingPath = pathValue.Substring(versionEndIndex);
        return remainingPath.StartsWith($"/{FeatureBucketPaths.Solution}/", StringComparison.OrdinalIgnoreCase);
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

        await WriteJsonResponseAsync(context, errorResponse);
    }

    private static async Task WriteForbiddenResponseAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json";

        var errorResponse = ErrorResponseDto.ForbiddenError(
            message,
            context.Items["CorrelationId"]?.ToString());

        await WriteJsonResponseAsync(context, errorResponse);
    }

    private static async Task WriteJsonResponseAsync(HttpContext context, ErrorResponseDto errorResponse)
    {
        var json = JsonSerializer.Serialize(
            errorResponse,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

        await context.Response.WriteAsync(json);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip if not API request or solution feature bucket
        if (!IsApiRequest(context.Request.Path) || IsSolutionFeatureBucketRequest(context.Request.Path))
        {
            await next(context);
            return;
        }

        // Skip if user is not authenticated (will be handled by [Authorize])
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await next(context);
            return;
        }

        // Allow superadmin to access any tenant with any header
        if (context.User.IsInRole(SuperAdminRole))
        {
            var superAdminTenantId = GetTenantIdFromHeader(context);
            if (superAdminTenantId.HasValue)
            {
                context.Items[ValidatedTenantIdKey] = superAdminTenantId.Value;
                logger.LogDebug("SuperAdmin user accessing tenant {TenantId}", superAdminTenantId.Value);
            }
            else
            {
                await WriteBadRequestResponseAsync(context, "SuperAdmin must provide X-Tenant-Id header to specify target tenant");
                return;
            }

            await next(context);
            return;
        }

        // Resolve tenant for regular users
        var tenantResolutionResult = ResolveTenantForUser(context);
        if (!tenantResolutionResult.Success)
        {
            if (tenantResolutionResult.IsForbidden)
            {
                await WriteForbiddenResponseAsync(context, tenantResolutionResult.ErrorMessage);
            }
            else
            {
                await WriteBadRequestResponseAsync(context, tenantResolutionResult.ErrorMessage);
            }
            return;
        }

        context.Items[ValidatedTenantIdKey] = tenantResolutionResult.TenantId!.Value;
        logger.LogDebug(
            "Resolved tenant {TenantId} for user {UserId}",
            tenantResolutionResult.TenantId.Value,
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown");

        await next(context);
    }

    private TenantResolutionResult ResolveTenantForUser(HttpContext context)
    {
        var headerTenantId = GetTenantIdFromHeader(context);
        var userTenantIds = GetTenantIdsFromClaims(context.User);

        // No tenant claims - only valid for solution feature bucket (already checked)
        if (!userTenantIds.Any())
        {
            return TenantResolutionResult.CreateForbidden("User has no tenant claims. Access restricted to solution endpoints only.");
        }

        // Single tenant claim
        if (userTenantIds.Count == 1)
        {
            var singleTenantId = userTenantIds.First();

            if (!headerTenantId.HasValue)
            {
                // No header - use the single tenant claim
                return TenantResolutionResult.CreateSuccess(singleTenantId);
            }

            return headerTenantId.Value == singleTenantId ?
                // Header matches single tenant claim
                TenantResolutionResult.CreateSuccess(singleTenantId) :
                // Header doesn't match the single tenant claim
                TenantResolutionResult.CreateForbidden($"X-Tenant-Id header {headerTenantId.Value} does not match user's authorized tenant {singleTenantId}");
        }

        // Multiple tenant claims
        if (!headerTenantId.HasValue)
        {
            return TenantResolutionResult.CreateBadRequest($"User has access to multiple tenants. X-Tenant-Id header is required to specify target tenant. Available tenants: {string.Join(", ", userTenantIds)}");
        }

        return userTenantIds.Contains(headerTenantId.Value) ?
            // Header matches one of the user's tenant claims
            TenantResolutionResult.CreateSuccess(headerTenantId.Value) :
            // Header doesn't match any of the user's tenant claims
            TenantResolutionResult.CreateForbidden($"X-Tenant-Id header {headerTenantId.Value} does not match any of user's authorized tenants: {string.Join(", ", userTenantIds)}");
    }
}

public class TenantResolutionResult
{
    public bool Success { get; private set; }
    public Guid? TenantId { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    public bool IsForbidden { get; private set; }

    public static TenantResolutionResult CreateSuccess(Guid tenantId)
        => new()
        {
            Success = true,
            TenantId = tenantId,
        };

    public static TenantResolutionResult CreateBadRequest(string message)
        => new()
        {
            Success = false,
            ErrorMessage = message,
            IsForbidden = false,
        };

    public static TenantResolutionResult CreateForbidden(string message)
        => new()
        {
            Success = false,
            ErrorMessage = message,
            IsForbidden = true,
        };
}
