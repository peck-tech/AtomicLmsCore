using AtomicLmsCore.Application.Common.Interfaces;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.LearningObjects.Commands;

/// <summary>
///     Handler for DeleteLearningObjectCommand.
/// </summary>
public class DeleteLearningObjectCommandHandler(ILearningObjectRepository learningObjectRepository)
    : IRequestHandler<DeleteLearningObjectCommand, Result>
{
    /// <summary>
    ///     Handles the deletion of a learning object.
    /// </summary>
    /// <param name="request">The delete learning object command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result> Handle(DeleteLearningObjectCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await learningObjectRepository.DeleteAsync(request.Id, cancellationToken);

            return !deleted ? Result.Fail($"Learning object with ID {request.Id} not found.") : Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete learning object: {ex.Message}");
        }
    }
}
