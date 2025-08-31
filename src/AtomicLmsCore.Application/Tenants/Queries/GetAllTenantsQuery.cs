using AtomicLmsCore.Domain.Entities;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Tenants.Queries;

/// <summary>
///     Query to get all tenants.
/// </summary>
public record GetAllTenantsQuery : IRequest<Result<IEnumerable<Tenant>>>;
