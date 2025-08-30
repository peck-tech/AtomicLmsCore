using System.ComponentModel.DataAnnotations;

namespace AtomicLmsCore.WebApi.Common;

/// <summary>
/// Standard error response for API endpoints.
/// </summary>
/// <param name="Type">Error category (e.g., Validation, Business, System).</param>
/// <param name="Title">Human-readable summary of the error.</param>
/// <param name="Status">HTTP status code.</param>
/// <param name="Errors">List of detailed error messages.</param>
/// <param name="CorrelationId">Optional correlation identifier for request tracing.</param>
public record ErrorResponseDto(
    [property: Required] string Type,
    [property: Required] string Title,
    [property: Required] int Status,
    [property: Required] List<string> Errors,
    string? CorrelationId = null)
{

    /// <summary>
    /// Creates a validation error response.
    /// </summary>
    public static ErrorResponseDto ValidationError(List<string> errors, string? correlationId = null)
        => new("Validation", "One or more validation errors occurred.", 400, errors, correlationId);

    /// <summary>
    /// Creates a business logic error response.
    /// </summary>
    public static ErrorResponseDto BusinessError(List<string> errors, string? correlationId = null)
        => new("Business", "Business rule violation.", 400, errors, correlationId);

    /// <summary>
    /// Creates a system error response.
    /// </summary>
    public static ErrorResponseDto SystemError(string? correlationId = null)
        => new("System", "An internal server error occurred.", 500,
            new List<string> { "An error occurred processing your request." }, correlationId);

    /// <summary>
    /// Creates a not found error response.
    /// </summary>
    public static ErrorResponseDto NotFoundError(string resource, string? correlationId = null)
        => new("NotFound", "Resource not found.", 404,
            new List<string> { $"{resource} not found." }, correlationId);
}
