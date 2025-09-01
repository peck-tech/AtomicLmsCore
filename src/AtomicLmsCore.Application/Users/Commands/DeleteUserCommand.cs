using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Users.Commands;

/// <summary>
///     Command to delete a user (soft delete).
/// </summary>
public record DeleteUserCommand(Guid Id) : IRequest<Result>;
