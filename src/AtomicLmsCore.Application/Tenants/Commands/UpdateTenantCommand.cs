using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Tenants.Commands;

/// <summary>
///     Command to update an existing tenant.
/// </summary>
/// <param name="Id">The unique identifier of the tenant to update.</param>
/// <param name="Name">The display name of the tenant.</param>
/// <param name="Slug">The unique slug/alias for the tenant.</param>
/// <param name="IsActive">Indicates whether the tenant is currently active.</param>
/// <param name="Metadata">Additional metadata for the tenant.</param>
public record UpdateTenantCommand(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    IDictionary<string, string>? Metadata = null) : IRequest<Result>;
