using AtomicLmsCore.Application.LearningObjects.Commands;
using AtomicLmsCore.Application.LearningObjects.Queries;
using AtomicLmsCore.WebApi.Common;
using AtomicLmsCore.WebApi.DTOs.LearningObjects;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtomicLmsCore.WebApi.Controllers.Learning;

/// <summary>
///     Controller for managing learning objects in the Learning feature bucket.
///     Requires authentication for all operations.
/// </summary>
[ApiController]
[ApiVersion("0.1")]
[Route(FeatureBucketPaths.LearningRoute)]
[Authorize]
public class LearningObjectsController(IMediator mediator, ILogger<LearningObjectsController> logger, IMapper mapper)
    : ControllerBase
{
    /// <summary>
    ///     Gets all learning objects for the current tenant.
    /// </summary>
    /// <returns>List of all learning objects.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LearningObjectListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var query = new GetAllLearningObjectsQuery();
            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                var learningObjectDtos = mapper.Map<IEnumerable<LearningObjectListDto>>(result.Value);
                return Ok(learningObjectDtos);
            }

            logger.LogWarning(
                "Failed to retrieve learning objects: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Message)));

            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving learning objects");
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Gets a learning object by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the learning object.</param>
    /// <returns>The learning object information.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LearningObjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var query = new GetLearningObjectByIdQuery(id);
            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                var learningObjectDto = mapper.Map<LearningObjectDto>(result.Value);
                return Ok(learningObjectDto);
            }

            if (result.Errors.Any(e => e.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                var notFoundResponse =
                    ErrorResponseDto.NotFoundError("Learning Object", HttpContext.Items["CorrelationId"]?.ToString());
                return NotFound(notFoundResponse);
            }

            logger.LogWarning(
                "Failed to retrieve learning object {LearningObjectId}: {Errors}",
                id,
                string.Join(", ", result.Errors.Select(e => e.Message)));

            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving learning object {LearningObjectId}", id);
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Creates a new learning object.
    /// </summary>
    /// <param name="request">The learning object creation request.</param>
    /// <returns>The created learning object's ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateLearningObjectRequestDto request)
    {
        try
        {
            var command = new CreateLearningObjectCommand(
                request.Name,
                request.Metadata);

            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetById),
                    new
                    {
                        id = result.Value,
                    },
                    result.Value);
            }

            logger.LogWarning(
                "Failed to create learning object: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Message)));

            var errorResponse = ErrorResponseDto.ValidationError(
                result.Errors.Select(e => e.Message).ToList(),
                HttpContext.Items["CorrelationId"]?.ToString());
            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating learning object");
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Updates an existing learning object.
    /// </summary>
    /// <param name="id">The unique identifier of the learning object to update.</param>
    /// <param name="request">The learning object update request.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLearningObjectRequestDto request)
    {
        try
        {
            var command = new UpdateLearningObjectCommand(
                id,
                request.Name,
                request.Metadata);

            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            if (result.Errors.Any(e => e.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                var notFoundResponse =
                    ErrorResponseDto.NotFoundError("Learning Object", HttpContext.Items["CorrelationId"]?.ToString());
                return NotFound(notFoundResponse);
            }

            logger.LogWarning(
                "Failed to update learning object {LearningObjectId}: {Errors}",
                id,
                string.Join(", ", result.Errors.Select(e => e.Message)));

            var errorResponse = ErrorResponseDto.ValidationError(
                result.Errors.Select(e => e.Message).ToList(),
                HttpContext.Items["CorrelationId"]?.ToString());
            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating learning object {LearningObjectId}", id);
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Deletes (soft delete) an existing learning object.
    /// </summary>
    /// <param name="id">The unique identifier of the learning object to delete.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteLearningObjectCommand(id);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            if (result.Errors.Any(e => e.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                var notFoundResponse =
                    ErrorResponseDto.NotFoundError("Learning Object", HttpContext.Items["CorrelationId"]?.ToString());
                return NotFound(notFoundResponse);
            }

            logger.LogWarning(
                "Failed to delete learning object {LearningObjectId}: {Errors}",
                id,
                string.Join(", ", result.Errors.Select(e => e.Message)));

            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting learning object {LearningObjectId}", id);
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}
