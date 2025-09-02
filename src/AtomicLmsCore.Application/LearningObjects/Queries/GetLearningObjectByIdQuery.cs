using AtomicLmsCore.Domain.Entities;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.LearningObjects.Queries;

/// <summary>
///     Query to get a learning object by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the learning object.</param>
public record GetLearningObjectByIdQuery(Guid Id) : IRequest<Result<LearningObject>>;
