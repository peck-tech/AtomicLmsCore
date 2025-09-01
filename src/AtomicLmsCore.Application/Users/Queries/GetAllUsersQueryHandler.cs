using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Users.Queries;

/// <summary>
///     Handler for getting all users in the current tenant database.
/// </summary>
public class GetAllUsersQueryHandler(
    IUserRepository userRepository,
    ILogger<GetAllUsersQueryHandler> logger)
    : IRequestHandler<GetAllUsersQuery, Result<IEnumerable<User>>>
{
    public async Task<Result<IEnumerable<User>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await userRepository.GetAllAsync();
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users");
            return Result.Fail<IEnumerable<User>>("Failed to retrieve users");
        }
    }
}
