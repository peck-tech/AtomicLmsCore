using System.ComponentModel.DataAnnotations;

namespace AtomicLmsCore.WebApi.DTOs.LearningObjects;

/// <summary>
///     Data transfer object for updating a learning object.
/// </summary>
/// <param name="Name">The updated name of the learning object.</param>
/// <param name="Metadata">Updated metadata for the learning object.</param>
public record UpdateLearningObjectRequestDto(
    [property: Required] string Name,
    IDictionary<string, string>? Metadata = null);
