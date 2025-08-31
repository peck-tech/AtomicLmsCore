using AtomicLmsCore.Domain.Entities;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Tenants.Queries;

/// <summary>
///     Query to get a tenant by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the tenant.</param>
public record GetTenantByIdQuery(Guid Id) : IRequest<Result<Tenant>>;
