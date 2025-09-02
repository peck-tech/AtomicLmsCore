using System.ComponentModel.DataAnnotations;

namespace AtomicLmsCore.WebApi.DTOs.LearningObjects;

/// <summary>
///     Data transfer object for creating a new learning object.
/// </summary>
/// <param name="Name">The name of the learning object.</param>
/// <param name="Metadata">Additional metadata for the learning object.</param>
public record CreateLearningObjectRequestDto(
    [property: Required] string Name,
    IDictionary<string, string>? Metadata = null);
