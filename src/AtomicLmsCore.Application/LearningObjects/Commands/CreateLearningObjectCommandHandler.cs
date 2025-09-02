using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.LearningObjects.Commands;

/// <summary>
///     Handler for CreateLearningObjectCommand.
/// </summary>
public class CreateLearningObjectCommandHandler(
    ILearningObjectRepository learningObjectRepository,
    IIdGenerator idGenerator)
    : IRequestHandler<CreateLearningObjectCommand, Result<Guid>>
{
    /// <summary>
    ///     Handles the creation of a new learning object.
    /// </summary>
    /// <param name="request">The create learning object command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the new learning object's ID or errors.</returns>
    public async Task<Result<Guid>> Handle(CreateLearningObjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var learningObject = new LearningObject
            {
                Id = idGenerator.NewId(),
                Name = request.Name,
                Metadata = request.Metadata ?? new Dictionary<string, string>(),
            };

            await learningObjectRepository.AddAsync(learningObject, cancellationToken);

            return Result.Ok(learningObject.Id);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create learning object: {ex.Message}");
        }
    }
}
