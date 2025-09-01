using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Users.Commands;

/// <summary>
///     Handler for deleting a user (soft delete).
/// </summary>
public class DeleteUserCommandHandler(
    IUserRepository userRepository,
    ILogger<DeleteUserCommandHandler> logger)
    : IRequestHandler<DeleteUserCommand, Result>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var deleteResult = await userRepository.DeleteAsync(request.Id);
            if (deleteResult.IsFailed)
            {
                return Result.Fail(deleteResult.Errors);
            }

            logger.LogInformation("User deleted successfully with ID {UserId}", request.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user {UserId}", request.Id);
            return Result.Fail("Failed to delete user");
        }
    }
}
