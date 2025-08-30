using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Tenants.Services;

public class TenantService(ITenantRepository tenantRepository, ILogger<TenantService> logger)
    : ITenantService
{
    public async Task<Result<Tenant>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await tenantRepository.GetByIdAsync(id, cancellationToken);

            return tenant == null ? Result.Fail<Tenant>($"Tenant with ID {id} not found") : Result.Ok(tenant);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tenant with ID {TenantId}", id);
            return Result.Fail<Tenant>($"An error occurred while retrieving the tenant: {ex.Message}");
        }
    }

    public async Task<Result<List<Tenant>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tenants = await tenantRepository.GetAllAsync(cancellationToken);
            return Result.Ok(tenants);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all tenants");
            return Result.Fail<List<Tenant>>($"An error occurred while retrieving tenants: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Fail<Guid>("Tenant name is required");
            }

            var tenant = new Tenant { Name = name };
            var createdTenant = await tenantRepository.AddAsync(tenant, cancellationToken);

            logger.LogInformation(
                "Created tenant with ID {TenantId} and name {TenantName}",
                createdTenant.Id,
                createdTenant.Name);
            return Result.Ok(createdTenant.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating tenant with name {TenantName}", name);
            return Result.Fail<Guid>($"An error occurred while creating the tenant: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(Guid id, string name, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Fail("Tenant name is required");
            }

            var tenant = await tenantRepository.GetByIdAsync(id, cancellationToken);

            if (tenant == null)
            {
                return Result.Fail($"Tenant with ID {id} not found");
            }

            tenant.Name = name;
            await tenantRepository.UpdateAsync(tenant, cancellationToken);

            logger.LogInformation("Updated tenant with ID {TenantId}", id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating tenant with ID {TenantId}", id);
            return Result.Fail($"An error occurred while updating the tenant: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await tenantRepository.DeleteAsync(id, cancellationToken);

            if (!deleted)
            {
                return Result.Fail($"Tenant with ID {id} not found");
            }

            logger.LogInformation("Deleted tenant with ID {TenantId}", id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting tenant with ID {TenantId}", id);
            return Result.Fail($"An error occurred while deleting the tenant: {ex.Message}");
        }
    }
}
