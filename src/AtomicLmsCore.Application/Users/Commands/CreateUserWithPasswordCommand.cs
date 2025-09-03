using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Users.Commands;

/// <summary>
///     Command to create a new user in both Auth0 and the local database.
/// </summary>
public record CreateUserWithPasswordCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string DisplayName,
    bool IsActive,
    IDictionary<string, string>? Metadata) : IRequest<Result<Guid>>;
