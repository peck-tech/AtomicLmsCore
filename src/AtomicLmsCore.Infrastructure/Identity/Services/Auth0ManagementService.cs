using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Infrastructure.Identity.Configuration;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AtomicLmsCore.Infrastructure.Identity.Services;

/// <summary>
///     Auth0 implementation of the identity management service.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="Auth0ManagementService"/> class.
/// </remarks>
/// <param name="auth0Options">The Auth0 configuration options.</param>
/// <param name="tokenService">The token service for getting access tokens.</param>
/// <param name="logger">The logger instance.</param>
public class Auth0ManagementService(
    IOptions<Auth0Options> auth0Options,
    IIdentityTokenService tokenService,
    ILogger<Auth0ManagementService> logger) : IIdentityManagementService
{
    private readonly Auth0Options _auth0Options = auth0Options.Value;

    /// <summary>
    ///     Gets a user by their identity provider user ID.
    /// </summary>
    /// <param name="userId">The identity provider user ID.</param>
    /// <returns>A result containing the user identity or error information.</returns>
    public async Task<Result<IdentityUser>> GetUserAsync(string userId)
    {
        try
        {
            var client = await GetManagementApiClientAsync();
            if (client.IsFailed)
            {
                return Result.Fail<IdentityUser>(client.Errors);
            }

            var user = await client.Value.Users.GetAsync(userId);
            return Result.Ok(MapToIdentityUser(user));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user {UserId} from Auth0", userId);
            return Result.Fail<IdentityUser>($"Error getting user from Auth0: {ex.Message}");
        }
    }

    /// <summary>
    ///     Creates a new user in the identity provider.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>A result containing the created user identity or error information.</returns>
    public async Task<Result<IdentityUser>> CreateUserAsync(string email, string password)
    {
        try
        {
            var client = await GetManagementApiClientAsync();
            if (client.IsFailed)
            {
                return Result.Fail<IdentityUser>(client.Errors);
            }

            var userCreateRequest = new UserCreateRequest
            {
                Email = email,
                Password = password,
                Connection = "Username-Password-Authentication",
                EmailVerified = false,
            };

            var user = await client.Value.Users.CreateAsync(userCreateRequest);
            return Result.Ok(MapToIdentityUser(user));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user with email {Email} in Auth0", email);
            return Result.Fail<IdentityUser>($"Error creating user in Auth0: {ex.Message}");
        }
    }

    /// <summary>
    ///     Updates a user's metadata in the identity provider.
    /// </summary>
    /// <param name="userId">The identity provider user ID.</param>
    /// <param name="metadata">The metadata to update.</param>
    /// <returns>A result containing the updated user identity or error information.</returns>
    public async Task<Result<IdentityUser>> UpdateUserMetadataAsync(string userId, Dictionary<string, object> metadata)
    {
        try
        {
            var client = await GetManagementApiClientAsync();
            if (client.IsFailed)
            {
                return Result.Fail<IdentityUser>(client.Errors);
            }

            var userUpdateRequest = new UserUpdateRequest
            {
                UserMetadata = metadata,
            };

            var user = await client.Value.Users.UpdateAsync(userId, userUpdateRequest);
            return Result.Ok(MapToIdentityUser(user));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId} metadata in Auth0", userId);
            return Result.Fail<IdentityUser>($"Error updating user metadata in Auth0: {ex.Message}");
        }
    }

    /// <summary>
    ///     Assigns a role to a user.
    /// </summary>
    /// <param name="userId">The Auth0 user ID.</param>
    /// <param name="roleId">The Auth0 role ID.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<Result> AssignRoleToUserAsync(string userId, string roleId)
    {
        try
        {
            var client = await GetManagementApiClientAsync();
            if (client.IsFailed)
            {
                return Result.Fail(client.Errors);
            }

            var assignRolesRequest = new AssignRolesRequest
            {
                Roles =
                [
                    roleId
                ],
            };

            await client.Value.Users.AssignRolesAsync(userId, assignRolesRequest);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning role {RoleId} to user {UserId} in Auth0", roleId, userId);
            return Result.Fail($"Error assigning role to user in Auth0: {ex.Message}");
        }
    }

    /// <summary>
    ///     Gets all roles for a user.
    /// </summary>
    /// <param name="userId">The identity provider user ID.</param>
    /// <returns>A result containing the user's roles or error information.</returns>
    public async Task<Result<IList<IdentityRole>>> GetUserRolesAsync(string userId)
    {
        try
        {
            var client = await GetManagementApiClientAsync();
            if (client.IsFailed)
            {
                return Result.Fail<IList<IdentityRole>>(client.Errors);
            }

            var pagedRoles = await client.Value.Users.GetRolesAsync(userId);
            var rolesList = pagedRoles.Select(MapToIdentityRole).ToList() as IList<IdentityRole>;
            return Result.Ok(rolesList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting roles for user {UserId} from Auth0", userId);
            return Result.Fail<IList<IdentityRole>>($"Error getting user roles from Auth0: {ex.Message}");
        }
    }

    private static IdentityUser MapToIdentityUser(User auth0User)
    {
        var identityUser = new IdentityUser
        {
            Id = auth0User.UserId,
            Email = auth0User.Email,
            EmailVerified = auth0User.EmailVerified ?? false,
            Metadata = new Dictionary<string, object>(),
        };

        if (auth0User.UserMetadata == null)
        {
            return identityUser;
        }

        foreach (var kvp in auth0User.UserMetadata)
        {
            identityUser.Metadata[kvp.Key] = kvp.Value;
        }

        return identityUser;
    }

    private static IdentityRole MapToIdentityRole(Role auth0Role)
        => new()
        {
            Id = auth0Role.Id,
            Name = auth0Role.Name,
            Description = auth0Role.Description,
        };

    private async Task<Result<ManagementApiClient>> GetManagementApiClientAsync()
    {
        var tokenResult = await tokenService.GetManagementTokenAsync();
        if (tokenResult.IsFailed)
        {
            return Result.Fail<ManagementApiClient>(tokenResult.Errors);
        }

        var client = new ManagementApiClient(tokenResult.Value, _auth0Options.Domain);
        return Result.Ok(client);
    }
}
