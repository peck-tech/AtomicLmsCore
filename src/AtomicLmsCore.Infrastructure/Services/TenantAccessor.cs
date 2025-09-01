using AtomicLmsCore.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AtomicLmsCore.Infrastructure.Services;

/// <summary>
///     Provides access to the current tenant context from HTTP headers.
/// </summary>
public class TenantAccessor(IHttpContextAccessor httpContextAccessor) : ITenantAccessor
{
    private const string TenantIdHeaderName = "X-Tenant-Id";

    /// <inheritdoc />
    public Guid? GetCurrentTenantId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.TryGetValue(TenantIdHeaderName, out var tenantIdHeader) == true &&
            Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            return tenantId;
        }

        return null;
    }

    /// <inheritdoc />
    public Guid GetRequiredCurrentTenantId()
    {
        var tenantId = GetCurrentTenantId();
        if (tenantId.HasValue)
        {
            return tenantId.Value;
        }

        throw new InvalidOperationException($"No valid tenant ID found in {TenantIdHeaderName} header");
    }
}
