using System.ComponentModel.DataAnnotations;

namespace AtomicLmsCore.WebApi.DTOs.LearningObjects;

/// <summary>
///     Data transfer object for learning object information.
/// </summary>
/// <param name="Id">The unique identifier of the learning object.</param>
/// <param name="Name">The name of the learning object.</param>
/// <param name="Metadata">Additional metadata for the learning object.</param>
/// <param name="CreatedAt">The date and time when the learning object was created.</param>
/// <param name="UpdatedAt">The date and time when the learning object was last updated.</param>
public record LearningObjectDto(
    [property: Required] Guid Id,
    [property: Required] string Name,
    [property: Required] IDictionary<string, string> Metadata,
    [property: Required] DateTime CreatedAt,
    [property: Required] DateTime UpdatedAt);
