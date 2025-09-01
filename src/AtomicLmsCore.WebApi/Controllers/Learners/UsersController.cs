using AtomicLmsCore.Application.Users.Commands;
using AtomicLmsCore.Application.Users.Queries;
using AtomicLmsCore.WebApi.Common;
using AtomicLmsCore.WebApi.DTOs.Users;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtomicLmsCore.WebApi.Controllers.Learners;

/// <summary>
///     Controller for managing users in the Learners feature bucket.
///     Uses tenant-specific database based on X-Tenant-Id header.
/// </summary>
[ApiController]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/learners/[controller]")]
[Authorize]
public class UsersController(IMediator mediator, ILogger<UsersController> logger, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Gets all users in the tenant database.
    /// </summary>
    /// <returns>List of all users in the tenant.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var query = new GetAllUsersQuery();
            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                var userDtos = mapper.Map<IEnumerable<UserListDto>>(result.Value);
                return Ok(userDtos);
            }

            logger.LogWarning(
                "Failed to retrieve users: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Message)));

            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users");
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The user information.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var query = new GetUserByIdQuery(id);
            var result = await mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                var userDto = mapper.Map<UserDto>(result.Value);
                return Ok(userDto);
            }

            if (result.IsSuccess && result.Value == null)
            {
                var notFoundResponse =
                    ErrorResponseDto.NotFoundError("User", HttpContext.Items["CorrelationId"]?.ToString());
                return NotFound(notFoundResponse);
            }

            logger.LogWarning(
                "Failed to retrieve user {UserId}: {Errors}",
                id,
                string.Join(", ", result.Errors.Select(e => e.Message)));

            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user {UserId}", id);
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Creates a new user in the tenant database.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <returns>The created user's ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
    {
        try
        {
            var command = new CreateUserCommand(
                request.ExternalUserId,
                request.Email,
                request.FirstName,
                request.LastName,
                request.DisplayName,
                request.IsActive,
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
                "Failed to create user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Message)));

            var errorResponse = ErrorResponseDto.ValidationError(
                result.Errors.Select(e => e.Message).ToList(),
                HttpContext.Items["CorrelationId"]?.ToString());
            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Updates an existing user.
    /// </summary>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="request">The user update request.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequestDto request)
    {
        try
        {
            var command = new UpdateUserCommand(
                id,
                request.Email,
                request.FirstName,
                request.LastName,
                request.DisplayName,
                request.IsActive,
                request.Metadata);

            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            if (result.Errors.Any(e => e.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                var notFoundResponse =
                    ErrorResponseDto.NotFoundError("User", HttpContext.Items["CorrelationId"]?.ToString());
                return NotFound(notFoundResponse);
            }

            logger.LogWarning(
                "Failed to update user {UserId}: {Errors}",
                id,
                string.Join(", ", result.Errors.Select(e => e.Message)));

            var errorResponse = ErrorResponseDto.ValidationError(
                result.Errors.Select(e => e.Message).ToList(),
                HttpContext.Items["CorrelationId"]?.ToString());
            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId}", id);
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Deletes (soft delete) an existing user.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteUserCommand(id);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            if (result.Errors.Any(e => e.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                var notFoundResponse =
                    ErrorResponseDto.NotFoundError("User", HttpContext.Items["CorrelationId"]?.ToString());
                return NotFound(notFoundResponse);
            }

            logger.LogWarning(
                "Failed to delete user {UserId}: {Errors}",
                id,
                string.Join(", ", result.Errors.Select(e => e.Message)));

            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user {UserId}", id);
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}
