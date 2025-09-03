using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Users.Commands;

/// <summary>
///     Handler for deleting a user (soft delete).
/// </summary>
[UsedImplicitly]
public class DeleteUserCommandHandler(
    IUserRepository userRepository,
    ILogger<DeleteUserCommandHandler> logger)
    : IRequestHandler<DeleteUserCommand, Result>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user info before deletion for Auth0 sync logging
            var userResult = await userRepository.GetByIdAsync(request.Id);
            string? externalUserId = null;

            if (userResult.IsSuccess && userResult.Value != null)
            {
                externalUserId = userResult.Value.ExternalUserId;
            }

            var deleteResult = await userRepository.DeleteAsync(request.Id);
            if (deleteResult.IsFailed)
            {
                return Result.Fail(deleteResult.Errors);
            }

            // Log information about the Auth0 user but don't delete from Auth0
            // In most cases, you don't want to delete Auth0 users when doing soft deletes
            // as they may have active sessions or need to be restored later
            if (!string.IsNullOrEmpty(externalUserId))
            {
                logger.LogInformation(
                    "User deleted from local database. Auth0 user {ExternalUserId} remains active. " +
                    "Consider implementing Auth0 user management policies if needed.",
                    externalUserId);
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
