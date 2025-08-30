using AtomicLmsCore.Domain.Entities;
using FluentResults;

namespace AtomicLmsCore.Application.Common.Interfaces;

/// <summary>
///     Service for managing tenant operations.
/// </summary>
public interface ITenantService
{
    /// <summary>
    ///     Retrieves a tenant by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tenant.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A result containing the tenant if found, or a failure result if not found or an error occurs.</returns>
    Task<Result<Tenant>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves all tenants in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A result containing a list of all tenants, or a failure result if an error occurs.</returns>
    Task<Result<List<Tenant>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new tenant with the specified name.
    /// </summary>
    /// <param name="name">The name of the tenant to create.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A result containing the ID of the created tenant, or a failure result if validation fails or an error occurs.</returns>
    Task<Result<Guid>> CreateAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing tenant's name.
    /// </summary>
    /// <param name="id">The unique identifier of the tenant to update.</param>
    /// <param name="name">The new name for the tenant.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>
    ///     A success result if the update succeeds, or a failure result if the tenant is not found, validation fails, or
    ///     an error occurs.
    /// </returns>
    Task<Result> UpdateAsync(Guid id, string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a tenant by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tenant to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A success result if the deletion succeeds, or a failure result if the tenant is not found or an error occurs.</returns>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
