using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Users.Commands;

/// <summary>
///     Command to create a new user.
/// </summary>
public record CreateUserCommand(
    string ExternalUserId,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    bool IsActive,
    IDictionary<string, string>? Metadata) : IRequest<Result<Guid>>;
