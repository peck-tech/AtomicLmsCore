using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AtomicLmsCore.Application.Tenants.Queries;

/// <summary>
///     Handler for GetAllTenantsQuery.
/// </summary>
[UsedImplicitly]
public class GetAllTenantsQueryHandler(ITenantRepository tenantRepository)
    : IRequestHandler<GetAllTenantsQuery, Result<IEnumerable<Tenant>>>
{
    /// <summary>
    ///     Handles the retrieval of all tenants.
    /// </summary>
    /// <param name="request">The get all tenants query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing all tenants or error information.</returns>
    public async Task<Result<IEnumerable<Tenant>>> Handle(
        GetAllTenantsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var tenants = await tenantRepository.GetAllAsync(cancellationToken);
            return Result.Ok<IEnumerable<Tenant>>(tenants);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve tenants: {ex.Message}");
        }
    }
}
