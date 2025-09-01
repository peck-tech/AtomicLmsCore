using AtomicLmsCore.Domain.Entities;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.Users.Queries;

/// <summary>
///     Query to get all users in the current tenant database.
/// </summary>
public record GetAllUsersQuery() : IRequest<Result<IEnumerable<User>>>;
