using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Tenants.Commands;

/// <summary>
///     Handler for CreateTenantCommand.
/// </summary>
public class CreateTenantCommandHandler(ITenantRepository tenantRepository, IIdGenerator idGenerator)
    : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    /// <summary>
    ///     Handles the creation of a new tenant.
    /// </summary>
    /// <param name="request">The create tenant command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the new tenant's ID or errors.</returns>
    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = new Tenant
            {
                Id = idGenerator.NewId(),
                Name = request.Name,
                Slug = request.Slug,
                IsActive = request.IsActive,
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            await tenantRepository.AddAsync(tenant, cancellationToken);

            return Result.Ok(tenant.Id);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create tenant: {ex.Message}");
        }
    }
}
