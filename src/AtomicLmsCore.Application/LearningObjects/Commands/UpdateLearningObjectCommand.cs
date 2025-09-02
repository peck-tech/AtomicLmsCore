using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.LearningObjects.Commands;

/// <summary>
///     Command to update an existing learning object.
/// </summary>
/// <param name="Id">The unique identifier of the learning object to update.</param>
/// <param name="Name">The updated name of the learning object.</param>
/// <param name="Metadata">Updated metadata for the learning object.</param>
public record UpdateLearningObjectCommand(
    Guid Id,
    string Name,
    IDictionary<string, string>? Metadata = null) : IRequest<Result>;
