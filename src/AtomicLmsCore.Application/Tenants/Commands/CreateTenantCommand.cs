using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Tenants.Commands;

/// <summary>
///     Command to create a new tenant.
/// </summary>
/// <param name="Name">The display name of the tenant.</param>
/// <param name="Slug">The unique slug/alias for the tenant.</param>
/// <param name="IsActive">Indicates whether the tenant is currently active.</param>
/// <param name="Metadata">Additional metadata for the tenant.</param>
public record CreateTenantCommand(
    string Name,
    string Slug,
    bool IsActive = true,
    IDictionary<string, string>? Metadata = null) : IRequest<Result<Guid>>;
