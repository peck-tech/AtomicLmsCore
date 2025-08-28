using AtomicLmsCore.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AtomicLmsCore.Infrastructure.Services;

public class TenantService : ITenantService
{
    private const string TenantIdHeader = "X-Tenant-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentTenantId()
    {
        var tenantId = _httpContextAccessor.HttpContext?.Request.Headers[TenantIdHeader].FirstOrDefault();
        return tenantId ?? "default";
    }

    public Task<bool> ValidateTenantAsync(string tenantId)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(tenantId));
    }
}