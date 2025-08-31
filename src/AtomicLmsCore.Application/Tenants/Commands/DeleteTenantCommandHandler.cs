using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Tenants.Commands;

/// <summary>
///     Handler for DeleteTenantCommand.
/// </summary>
public class DeleteTenantCommandHandler(ITenantRepository tenantRepository)
    : IRequestHandler<DeleteTenantCommand, Result>
{
    /// <summary>
    ///     Handles the soft deletion of an existing tenant.
    /// </summary>
    /// <param name="request">The delete tenant command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await tenantRepository.GetByIdAsync(request.Id, cancellationToken);
            if (tenant == null)
            {
                return Result.Fail("Tenant not found");
            }

            await tenantRepository.DeleteAsync(request.Id, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete tenant: {ex.Message}");
        }
    }
}
