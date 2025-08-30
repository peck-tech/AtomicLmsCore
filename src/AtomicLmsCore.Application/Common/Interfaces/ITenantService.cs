using AtomicLmsCore.Domain.Entities;
using FluentResults;

namespace AtomicLmsCore.Application.Common.Interfaces;

public interface ITenantService
{
    Task<Result<Tenant>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<List<Tenant>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(string name, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, string name, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}