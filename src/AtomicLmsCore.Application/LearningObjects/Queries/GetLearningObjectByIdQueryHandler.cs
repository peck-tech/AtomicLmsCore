using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.LearningObjects.Queries;

/// <summary>
///     Handler for GetLearningObjectByIdQuery.
/// </summary>
public class GetLearningObjectByIdQueryHandler(ILearningObjectRepository learningObjectRepository)
    : IRequestHandler<GetLearningObjectByIdQuery, Result<LearningObject>>
{
    /// <summary>
    ///     Handles the retrieval of a learning object by its ID.
    /// </summary>
    /// <param name="request">The get learning object by ID query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the learning object or errors.</returns>
    public async Task<Result<LearningObject>> Handle(GetLearningObjectByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var learningObject = await learningObjectRepository.GetByIdAsync(request.Id, cancellationToken);

            return learningObject == null ? Result.Fail($"Learning object with ID {request.Id} not found.") : Result.Ok(learningObject);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve learning object: {ex.Message}");
        }
    }
}
