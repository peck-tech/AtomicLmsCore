using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Users.Commands;

/// <summary>
///     Handler for creating a new user.
/// </summary>
[UsedImplicitly]
public class CreateUserCommandHandler(
    IUserRepository userRepository,
    IIdentityManagementService identityManagementService,
    IIdGenerator idGenerator,
    ILogger<CreateUserCommandHandler> logger)
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailExistsResult = await userRepository.EmailExistsAsync(request.Email);
            if (emailExistsResult.IsFailed)
            {
                return Result.Fail<Guid>(emailExistsResult.Errors);
            }

            if (emailExistsResult.Value)
            {
                return Result.Fail<Guid>("A user with this email already exists in the tenant");
            }

            var externalUserExistsResult = await userRepository.ExternalUserIdExistsAsync(request.ExternalUserId);
            if (externalUserExistsResult.IsFailed)
            {
                return Result.Fail<Guid>(externalUserExistsResult.Errors);
            }

            if (externalUserExistsResult.Value)
            {
                return Result.Fail<Guid>("A user with this external user ID already exists");
            }

            var user = new User
            {
                Id = idGenerator.NewId(),
                ExternalUserId = request.ExternalUserId,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DisplayName = request.DisplayName,
                IsActive = request.IsActive,
                Metadata = request.Metadata ?? new Dictionary<string, string>(),
            };

            var addResult = await userRepository.AddAsync(user);
            if (addResult.IsFailed)
            {
                return Result.Fail<Guid>(addResult.Errors);
            }

            // Sync user metadata to Auth0 if metadata is provided
            if (request.Metadata != null && request.Metadata.Any())
            {
                var metadataDict = request.Metadata.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                var syncResult = await identityManagementService.UpdateUserMetadataAsync(request.ExternalUserId, metadataDict);
                if (syncResult.IsFailed)
                {
                    logger.LogWarning(
                        "Failed to sync metadata to identity provider for user {ExternalUserId}: {Errors}",
                        request.ExternalUserId,
                        string.Join(", ", syncResult.Errors.Select(e => e.Message)));
                    // Don't fail the entire operation if metadata sync fails
                }
                else
                {
                    logger.LogDebug("Successfully synced metadata to identity provider for user {ExternalUserId}", request.ExternalUserId);
                }
            }

            logger.LogInformation(
                "User created successfully with ID {UserId} and external ID {ExternalUserId}",
                user.Id,
                request.ExternalUserId);
            return Result.Ok(user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return Result.Fail<Guid>("Failed to create user");
        }
    }
}
