using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.LearningObjects.Commands;

/// <summary>
///     Handler for UpdateLearningObjectCommand.
/// </summary>
public class UpdateLearningObjectCommandHandler(ILearningObjectRepository learningObjectRepository)
    : IRequestHandler<UpdateLearningObjectCommand, Result>
{
    /// <summary>
    ///     Handles the update of an existing learning object.
    /// </summary>
    /// <param name="request">The update learning object command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result> Handle(UpdateLearningObjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var learningObject = await learningObjectRepository.GetByIdAsync(request.Id, cancellationToken);

            if (learningObject == null)
            {
                return Result.Fail($"Learning object with ID {request.Id} not found.");
            }

            learningObject.Name = request.Name;
            learningObject.Metadata = request.Metadata ?? new Dictionary<string, string>();

            await learningObjectRepository.UpdateAsync(learningObject, cancellationToken);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update learning object: {ex.Message}");
        }
    }
}
