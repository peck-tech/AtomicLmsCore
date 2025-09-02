using AtomicLmsCore.Domain.Entities;

namespace AtomicLmsCore.Application.Common.Interfaces;

public interface ILearningObjectRepository
{
    Task<LearningObject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<LearningObject>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LearningObject> AddAsync(LearningObject learningObject, CancellationToken cancellationToken = default);
    Task<LearningObject> UpdateAsync(LearningObject learningObject, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
