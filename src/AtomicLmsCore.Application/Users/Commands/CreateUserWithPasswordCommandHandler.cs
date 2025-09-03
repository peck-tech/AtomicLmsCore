using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Users.Commands;

/// <summary>
///     Handler for creating a new user in both Auth0 and the local database.
/// </summary>
[UsedImplicitly]
public class CreateUserWithPasswordCommandHandler(
    IUserRepository userRepository,
    IIdentityManagementService identityManagementService,
    IIdGenerator idGenerator,
    ILogger<CreateUserWithPasswordCommandHandler> logger)
    : IRequestHandler<CreateUserWithPasswordCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserWithPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating user with email {Email}", request.Email);

            // Check if user already exists in local database
            var emailExistsResult = await userRepository.EmailExistsAsync(request.Email);
            if (emailExistsResult.IsFailed)
            {
                return Result.Fail<Guid>(emailExistsResult.Errors);
            }

            if (emailExistsResult.Value)
            {
                return Result.Fail<Guid>("A user with this email already exists in the tenant");
            }

            // Create user in Auth0 first
            var identityUserResult = await identityManagementService.CreateUserAsync(request.Email, request.Password);
            if (identityUserResult.IsFailed)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "Failed to create user in identity provider: {Errors}",
                        string.Join(", ", identityUserResult.Errors.Select(e => e.Message)));
                }
                return Result.Fail<Guid>(identityUserResult.Errors);
            }

            var identityUser = identityUserResult.Value;

            try
            {
                // Sync user metadata to Auth0 if provided
                if (request.Metadata != null && request.Metadata.Any())
                {
                    var metadataDict = request.Metadata.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                    var syncResult = await identityManagementService.UpdateUserMetadataAsync(identityUser.Id, metadataDict);
                    if (syncResult.IsFailed)
                    {
                        if (logger.IsEnabled(LogLevel.Warning))
                        {
                            logger.LogWarning(
                                "Failed to sync metadata to identity provider for user {UserId}: {Errors}",
                                identityUser.Id,
                                string.Join(", ", syncResult.Errors.Select(e => e.Message)));
                        }
                    }
                }

                // Create user in local database
                var user = new User
                {
                    Id = idGenerator.NewId(),
                    ExternalUserId = identityUser.Id,
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
                    logger.LogError("Failed to create user in local database after creating in identity provider. User ID in identity provider: {ExternalUserId}", identityUser.Id);
                    return Result.Fail<Guid>(addResult.Errors);
                }

                logger.LogInformation(
                    "User created successfully with ID {UserId} and external ID {ExternalUserId}",
                    user.Id,
                    identityUser.Id);
                return Result.Ok(user.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating user in local database after successful Auth0 creation. External User ID: {ExternalUserId}", identityUser.Id);
                // Note: Auth0 user exists but local user creation failed
                // In a production system, you might want to implement compensation logic here
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user with password");
            return Result.Fail<Guid>("Failed to create user");
        }
    }
}
