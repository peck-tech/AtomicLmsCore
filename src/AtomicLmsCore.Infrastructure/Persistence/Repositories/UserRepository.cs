using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository implementation for User entity operations in tenant-specific database.
/// </summary>
public class UserRepository(TenantDbContext context, ILogger<UserRepository> logger) : IUserRepository
{
    /// <inheritdoc />
    public async Task<Result<IEnumerable<User>>> GetAllAsync()
    {
        try
        {
            var users = await context.Users
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            return Result.Ok<IEnumerable<User>>(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users");
            return Result.Fail<IEnumerable<User>>("Failed to retrieve users");
        }
    }

    /// <inheritdoc />
    public async Task<Result<User?>> GetByIdAsync(Guid id)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            return Result.Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user {UserId}", id);
            return Result.Fail<User?>("Failed to retrieve user");
        }
    }

    /// <inheritdoc />
    public async Task<Result<User?>> GetByExternalUserIdAsync(string externalUserId)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.ExternalUserId == externalUserId && !u.IsDeleted);

            return Result.Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user by external ID {ExternalUserId}", externalUserId);
            return Result.Fail<User?>("Failed to retrieve user");
        }
    }

    /// <inheritdoc />
    public async Task<Result<User?>> GetByEmailAsync(string email)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            return Result.Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user by email {Email}", email);
            return Result.Fail<User?>("Failed to retrieve user");
        }
    }

    /// <inheritdoc />
    public async Task<Result<User>> AddAsync(User user)
    {
        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return Result.Ok(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding user");
            return Result.Fail<User>("Failed to add user");
        }
    }

    /// <inheritdoc />
    public async Task<Result> UpdateAsync(User user)
    {
        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId}", user.Id);
            return Result.Fail("Failed to update user");
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
            {
                return Result.Fail("User not found");
            }

            context.Entry(user).Property("IsDeleted").CurrentValue = true;
            await context.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user {UserId}", id);
            return Result.Fail("Failed to delete user");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> EmailExistsAsync(string email, Guid? excludeUserId = null)
    {
        try
        {
            var query = context.Users
                .Where(u => u.Email == email && !u.IsDeleted);

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            var exists = await query.AnyAsync();
            return Result.Ok(exists);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking email existence {Email}", email);
            return Result.Fail<bool>("Failed to check email existence");
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool>> ExternalUserIdExistsAsync(string externalUserId)
    {
        try
        {
            var exists = await context.Users
                .AnyAsync(u => u.ExternalUserId == externalUserId && !u.IsDeleted);

            return Result.Ok(exists);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking external user ID existence {ExternalUserId}", externalUserId);
            return Result.Fail<bool>("Failed to check external user ID existence");
        }
    }
}
