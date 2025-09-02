using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AtomicLmsCore.Application.Tenants.Queries;

/// <summary>
///     Handler for GetTenantByIdQuery.
/// </summary>
[UsedImplicitly]
public class GetTenantByIdQueryHandler(ITenantRepository tenantRepository)
    : IRequestHandler<GetTenantByIdQuery, Result<Tenant>>
{
    /// <summary>
    ///     Handles the retrieval of a tenant by its unique identifier.
    /// </summary>
    /// <param name="request">The get tenant by ID query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the tenant or error information.</returns>
    public async Task<Result<Tenant>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await tenantRepository.GetByIdAsync(request.Id, cancellationToken);
            return tenant == null ? Result.Fail("Tenant not found") : Result.Ok(tenant);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve tenant: {ex.Message}");
        }
    }
}
