using AtomicLmsCore.Domain.Entities;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Users.Queries;

/// <summary>
///     Query to get a user by their unique identifier.
/// </summary>
public record GetUserByIdQuery(Guid Id) : IRequest<Result<User?>>;
