using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.LearningObjects.Commands;

/// <summary>
///     Command to delete a learning object.
/// </summary>
/// <param name="Id">The unique identifier of the learning object to delete.</param>
public record DeleteLearningObjectCommand(Guid Id) : IRequest<Result>;
