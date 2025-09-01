using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtomicLmsCore.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository for Tenant entity operations using the Solutions database.
/// </summary>
public class TenantRepository(SolutionsDbContext context) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Set<Tenant>()
            .Where(t => t.Id == id && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Tenant>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Set<Tenant>()
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public async Task<Tenant> AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        context.Set<Tenant>().Add(tenant);
        await context.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        context.Set<Tenant>().Update(tenant);
        await context.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await context.Set<Tenant>()
            .Where(t => t.Id == id && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant == null)
        {
            return false;
        }

        context.Entry(tenant).Property("IsDeleted").CurrentValue = true;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Set<Tenant>()
            .Where(t => t.Id == id && !t.IsDeleted)
            .AnyAsync(cancellationToken);
}
