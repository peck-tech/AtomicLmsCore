using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Tenants.Commands;

/// <summary>
///     Command to delete (soft delete) an existing tenant.
/// </summary>
/// <param name="Id">The unique identifier of the tenant to delete.</param>
public record DeleteTenantCommand(Guid Id) : IRequest<Result>;
