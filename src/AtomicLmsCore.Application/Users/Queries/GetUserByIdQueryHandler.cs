using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Users.Queries;

/// <summary>
///     Handler for getting a user by their unique identifier.
/// </summary>
[UsedImplicitly]
public class GetUserByIdQueryHandler(
    IUserRepository userRepository,
    ILogger<GetUserByIdQueryHandler> logger)
    : IRequestHandler<GetUserByIdQuery, Result<User?>>
{
    public async Task<Result<User?>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await userRepository.GetByIdAsync(request.Id);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user {UserId}", request.Id);
            return Result.Fail<User?>("Failed to retrieve user");
        }
    }
}
