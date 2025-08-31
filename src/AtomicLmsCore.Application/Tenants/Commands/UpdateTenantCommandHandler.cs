using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Tenants.Commands;

/// <summary>
///     Handler for UpdateTenantCommand.
/// </summary>
public class UpdateTenantCommandHandler(ITenantRepository tenantRepository)
    : IRequestHandler<UpdateTenantCommand, Result>
{
    /// <summary>
    ///     Handles the update of an existing tenant.
    /// </summary>
    /// <param name="request">The update tenant command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await tenantRepository.GetByIdAsync(request.Id, cancellationToken);
            if (tenant == null)
            {
                return Result.Fail("Tenant not found");
            }

            tenant.Name = request.Name;
            tenant.Slug = request.Slug;
            tenant.IsActive = request.IsActive;
            tenant.Metadata = request.Metadata ?? new Dictionary<string, string>();

            await tenantRepository.UpdateAsync(tenant, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update tenant: {ex.Message}");
        }
    }
}
