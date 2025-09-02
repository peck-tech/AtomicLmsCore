using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtomicLmsCore.Infrastructure.Persistence.Repositories;

/// <summary>
///     Repository for managing Learning Object entities in tenant-specific databases.
/// </summary>
public class LearningObjectRepository(TenantDbContext context) : ILearningObjectRepository
{
    /// <summary>
    ///     Gets a learning object by its unique identifier.
    /// </summary>
    public async Task<LearningObject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.LearningObjects
            .FirstOrDefaultAsync(lo => lo.Id == id, cancellationToken);

    /// <summary>
    ///     Gets all learning objects for the current tenant.
    /// </summary>
    public async Task<List<LearningObject>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.LearningObjects
            .OrderBy(lo => lo.Name)
            .ToListAsync(cancellationToken);

    /// <summary>
    ///     Adds a new learning object.
    /// </summary>
    public async Task<LearningObject> AddAsync(LearningObject learningObject, CancellationToken cancellationToken = default)
    {
        await context.LearningObjects.AddAsync(learningObject, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return learningObject;
    }

    /// <summary>
    ///     Updates an existing learning object.
    /// </summary>
    public async Task<LearningObject> UpdateAsync(LearningObject learningObject, CancellationToken cancellationToken = default)
    {
        context.LearningObjects.Update(learningObject);
        await context.SaveChangesAsync(cancellationToken);
        return learningObject;
    }

    /// <summary>
    ///     Soft deletes a learning object.
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var learningObject = await context.LearningObjects
            .FirstOrDefaultAsync(lo => lo.Id == id, cancellationToken);

        if (learningObject == null)
        {
            return false;
        }

        // Soft delete by setting IsDeleted flag
        context.Entry(learningObject).Property("IsDeleted").CurrentValue = true;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    ///     Checks if a learning object exists.
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.LearningObjects
            .AnyAsync(lo => lo.Id == id, cancellationToken);
}
