using System.ComponentModel.DataAnnotations;

namespace AtomicLmsCore.WebApi.Common;

/// <summary>
/// Standard error response for API endpoints.
/// </summary>
/// <param name="type">Error category (e.g., Validation, Business, System).</param>
/// <param name="title">Human-readable summary of the error.</param>
/// <param name="status">HTTP status code.</param>
/// <param name="errors">List of detailed error messages.</param>
/// <param name="correlationId">Optional correlation identifier for request tracing.</param>
public record ErrorResponseDto(
    [Required] string type,
    [Required] string title,
    [Required] int status,
    [Required] List<string> errors,
    string? correlationId = null)
{
    /// <summary>
    /// Creates a validation error response.
    /// </summary>
    public static ErrorResponseDto ValidationError(List<string> errors, string? correlationId = null)
        => new ("Validation", "One or more validation errors occurred.", 400, errors, correlationId);

    /// <summary>
    /// Creates a business logic error response.
    /// </summary>
    public static ErrorResponseDto BusinessError(List<string> errors, string? correlationId = null)
        => new ("Business", "Business rule violation.", 400, errors, correlationId);

    /// <summary>
    /// Creates a system error response.
    /// </summary>
    public static ErrorResponseDto SystemError(string? correlationId = null)
        => new ("System", "An internal server error occurred.", 500,
            new List<string> { "An error occurred processing your request." }, correlationId);

    /// <summary>
    /// Creates a not found error response.
    /// </summary>
    public static ErrorResponseDto NotFoundError(string resource, string? correlationId = null)
        => new ("NotFound", "Resource not found.", 404,
            new List<string> { $"{resource} not found." }, correlationId);
}
