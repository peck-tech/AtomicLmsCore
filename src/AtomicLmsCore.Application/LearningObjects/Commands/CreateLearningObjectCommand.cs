using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.LearningObjects.Commands;

/// <summary>
///     Command to create a new learning object.
/// </summary>
/// <param name="Name">The name of the learning object.</param>
/// <param name="Metadata">Additional metadata for the learning object.</param>
public record CreateLearningObjectCommand(
    string Name,
    IDictionary<string, string>? Metadata = null) : IRequest<Result<Guid>>;
