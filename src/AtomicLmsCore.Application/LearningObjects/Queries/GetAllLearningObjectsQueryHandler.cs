using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using FluentResults;
using JetBrains.Annotations;
using MediatR;

namespace AtomicLmsCore.Application.LearningObjects.Queries;

/// <summary>
///     Handler for GetAllLearningObjectsQuery.
/// </summary>
[UsedImplicitly]
public class GetAllLearningObjectsQueryHandler(ILearningObjectRepository learningObjectRepository)
    : IRequestHandler<GetAllLearningObjectsQuery, Result<List<LearningObject>>>
{
    /// <summary>
    ///     Handles the retrieval of all learning objects for the current tenant.
    /// </summary>
    /// <param name="request">The get all learning objects query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the list of learning objects or errors.</returns>
    public async Task<Result<List<LearningObject>>> Handle(GetAllLearningObjectsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var learningObjects = await learningObjectRepository.GetAllAsync(cancellationToken);
            return Result.Ok(learningObjects);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve learning objects: {ex.Message}");
        }
    }
}
