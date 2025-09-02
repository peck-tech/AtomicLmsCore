using AtomicLmsCore.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AtomicLmsCore.IntegrationTests.Common;

public class TestTenantAccessor(IHttpContextAccessor httpContextAccessor) : ITenantAccessor
{
    public Guid? GetCurrentTenantId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        // First, try to get from X-Test-Tenant header (for test authentication)
        if (httpContext.Request.Headers.TryGetValue("X-Test-Tenant", out var testTenantHeader))
        {
            if (Guid.TryParse(testTenantHeader.FirstOrDefault(), out var testTenantId))
            {
                return testTenantId;
            }
        }

        // Then try to get from tenant claims
        var tenantClaim = httpContext.User.FindFirst("tenant_id");
        if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var claimTenantId))
        {
            return claimTenantId;
        }

        // Finally, try to get from X-Tenant-Id header
        if (!httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader))
        {
            return null;
        }

        return Guid.TryParse(tenantHeader.FirstOrDefault(), out var headerTenantId) ? headerTenantId : null;
    }

    public Guid GetRequiredCurrentTenantId()
        => GetCurrentTenantId() ?? throw new InvalidOperationException("Current tenant not found");
}
