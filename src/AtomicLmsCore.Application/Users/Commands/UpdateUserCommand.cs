using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Users.Commands;

/// <summary>
///     Command to update an existing user.
/// </summary>
public record UpdateUserCommand(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    bool IsActive,
    IDictionary<string, string>? Metadata) : IRequest<Result>;
