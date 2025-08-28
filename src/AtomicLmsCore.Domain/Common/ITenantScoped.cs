namespace AtomicLmsCore.Domain.Common;

public interface ITenantScoped
{
    string TenantId { get; set; }
}