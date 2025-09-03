using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;

namespace AtomicLmsCore.IntegrationTests.Common;

/// <summary>
///     Test implementation of IIdentityManagementService for integration tests.
/// </summary>
public class TestIdentityManagementService : IIdentityManagementService
{
    /// <summary>
    ///     Gets a user by their identity provider user ID.
    /// </summary>
    public Task<Result<IdentityUser>> GetUserAsync(string userId)
    {
        var user = new IdentityUser
        {
            Id = userId,
            Email = $"test-{userId}@example.com",
            EmailVerified = true,
            Metadata = new Dictionary<string, object>
            {
                { "tenant_id", "test-tenant-id" }
            }
        };

        return Task.FromResult(Result.Ok(user));
    }

    /// <summary>
    ///     Creates a new user in the identity provider.
    /// </summary>
    public Task<Result<IdentityUser>> CreateUserAsync(string email, string password)
    {
        var user = new IdentityUser
        {
            Id = $"test-{Guid.NewGuid()}",
            Email = email,
            EmailVerified = true,
            Metadata = new Dictionary<string, object>()
        };

        return Task.FromResult(Result.Ok(user));
    }

    /// <summary>
    ///     Updates a user's metadata in the identity provider.
    /// </summary>
    public Task<Result<IdentityUser>> UpdateUserMetadataAsync(string userId, Dictionary<string, object> metadata)
    {
        var user = new IdentityUser
        {
            Id = userId,
            Email = $"test-{userId}@example.com",
            EmailVerified = true,
            Metadata = metadata
        };

        return Task.FromResult(Result.Ok(user));
    }

    /// <summary>
    ///     Assigns a role to a user.
    /// </summary>
    public Task<Result> AssignRoleToUserAsync(string userId, string roleId)
        => Task.FromResult(Result.Ok());

    /// <summary>
    ///     Gets all roles for a user.
    /// </summary>
    public Task<Result<IList<IdentityRole>>> GetUserRolesAsync(string userId)
    {
        var roles = new List<IdentityRole>
        {
            new()
            {
                Id = "test-role-id",
                Name = "TestRole",
                Description = "Test role for integration tests"
            }
        };

        return Task.FromResult(Result.Ok<IList<IdentityRole>>(roles));
    }
}
