using AtomicLmsCore.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AtomicLmsCore.Infrastructure.Services;

/// <summary>
///     Provides access to the current tenant context from validated middleware context.
/// </summary>
public class TenantAccessor(IHttpContextAccessor httpContextAccessor) : ITenantAccessor
{
    private const string ValidatedTenantIdKey = "ValidatedTenantId";

    /// <inheritdoc />
    public Guid? GetCurrentTenantId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Items.TryGetValue(ValidatedTenantIdKey, out var tenantIdItem) == true &&
            tenantIdItem is Guid tenantId)
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

        throw new InvalidOperationException("No valid tenant ID found. Ensure TenantValidationMiddleware is properly configured.");
    }
}
