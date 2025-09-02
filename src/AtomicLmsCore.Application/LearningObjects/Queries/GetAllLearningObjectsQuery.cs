using AtomicLmsCore.Domain.Entities;
using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.LearningObjects.Queries;

/// <summary>
///     Query to get all learning objects for the current tenant.
/// </summary>
public record GetAllLearningObjectsQuery : IRequest<Result<List<LearningObject>>>;
