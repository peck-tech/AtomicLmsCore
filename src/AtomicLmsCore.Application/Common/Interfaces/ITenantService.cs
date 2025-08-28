namespace AtomicLmsCore.Application.Common.Interfaces;

public interface ITenantService
{
    string GetCurrentTenantId();
    Task<bool> ValidateTenantAsync(string tenantId);
}