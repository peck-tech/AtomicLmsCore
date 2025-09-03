using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Users.Commands;

/// <summary>
///     Handler for updating an existing user.
/// </summary>
[UsedImplicitly]
public class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IIdentityManagementService identityManagementService,
    ILogger<UpdateUserCommandHandler> logger)
    : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userResult = await userRepository.GetByIdAsync(request.Id);
            if (userResult.IsFailed || userResult.Value == null)
            {
                return Result.Fail("User not found");
            }

            var user = userResult.Value;

            if (user.Email != request.Email)
            {
                var emailExistsResult = await userRepository.EmailExistsAsync(
                    request.Email,
                    request.Id);

                if (emailExistsResult.IsFailed)
                {
                    return Result.Fail(emailExistsResult.Errors);
                }

                if (emailExistsResult.Value)
                {
                    return Result.Fail("A user with this email already exists in the tenant");
                }
            }

            user.Email = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.DisplayName = request.DisplayName;
            user.IsActive = request.IsActive;

            if (request.Metadata != null)
            {
                user.Metadata = request.Metadata;
            }

            var updateResult = await userRepository.UpdateAsync(user);
            if (updateResult.IsFailed)
            {
                return Result.Fail(updateResult.Errors);
            }

            // Sync user metadata to Auth0 if metadata is provided
            if (request.Metadata != null && request.Metadata.Any())
            {
                var metadataDict = request.Metadata.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                var syncResult = await identityManagementService.UpdateUserMetadataAsync(user.ExternalUserId, metadataDict);
                if (syncResult.IsFailed)
                {
                    logger.LogWarning(
                        "Failed to sync metadata to identity provider for user {ExternalUserId}: {Errors}",
                        user.ExternalUserId,
                        string.Join(", ", syncResult.Errors.Select(e => e.Message)));
                    // Don't fail the entire operation if metadata sync fails
                }
                else
                {
                    logger.LogDebug("Successfully synced metadata to identity provider for user {ExternalUserId}", user.ExternalUserId);
                }
            }

            logger.LogInformation(
                "User updated successfully with ID {UserId} and external ID {ExternalUserId}",
                user.Id,
                user.ExternalUserId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId}", request.Id);
            return Result.Fail("Failed to update user");
        }
    }
}
