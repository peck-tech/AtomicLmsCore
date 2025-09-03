using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Tenants.Commands;
using AtomicLmsCore.Application.Tenants.Queries;
using AtomicLmsCore.WebApi.Authorization;
using AtomicLmsCore.WebApi.Common;
using AtomicLmsCore.WebApi.DTOs.Tenants;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtomicLmsCore.WebApi.Controllers.Solution;

/// <summary>
///     Controller for managing tenants in the Solution feature bucket.
///     Uses unified permission-based authorization supporting both user roles and machine scopes.
/// </summary>
[ApiController]
[ApiVersion("0.1")]
[Route(FeatureBucketPaths.SolutionRoute)]
[Authorize]
public class TenantsController(IMediator mediator, ILogger<TenantsController> logger, IMapper mapper) : ControllerBase
{
    /// <summary>
    ///     Gets all tenants.
    ///     Requires 'tenants:read' permission.
    /// </summary>
    /// <returns>List of all tenants.</returns>
    [HttpGet]
    [RequirePermission(Permissions.Tenants.Read)]
    [ProducesResponseType(typeof(IEnumerable<TenantListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var query = new GetAllTenantsQuery();
            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                var tenantDtos = mapper.Map<IEnumerable<TenantListDto>>(result.Value);
                return Ok(tenantDtos);
            }

            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    "Failed to retrieve tenants: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Message)));
            }

            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tenants");
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Gets a tenant by its unique identifier.
    ///     Requires 'tenants:read' permission.
    /// </summary>
    /// <param name="id">The unique identifier of the tenant.</param>
    /// <returns>The tenant information.</returns>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Tenants.Read)]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var query = new GetTenantByIdQuery(id);
            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                var tenantDto = mapper.Map<TenantDto>(result.Value);
                return Ok(tenantDto);
            }

            if (result.Errors.Any(e => e.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                var notFoundResponse =
                    ErrorResponseDto.NotFoundError("Tenant", HttpContext.Items["CorrelationId"]?.ToString());
                return NotFound(notFoundResponse);
            }

            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    "Failed to retrieve tenant {TenantId}: {Errors}",
                    id,
                    string.Join(", ", result.Errors.Select(e => e.Message)));
            }

            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tenant {TenantId}", id);
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Creates a new tenant.
    ///     Requires 'tenants:create' permission.
    /// </summary>
    /// <param name="request">The tenant creation request.</param>
    /// <returns>The created tenant's ID.</returns>
    [HttpPost]
    [RequirePermission(Permissions.Tenants.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequestDto request)
    {
        try
        {
            var command = new CreateTenantCommand(
                request.Name,
                request.Slug,
                request.DatabaseName,
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

            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    "Failed to create tenant: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Message)));
            }

            var errorResponse = ErrorResponseDto.ValidationError(
                result.Errors.Select(e => e.Message).ToList(),
                HttpContext.Items["CorrelationId"]?.ToString());
            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating tenant");
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Updates an existing tenant.
    ///     Requires 'tenants:update' permission.
    /// </summary>
    /// <param name="id">The unique identifier of the tenant to update.</param>
    /// <param name="request">The tenant update request.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.Tenants.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequestDto request)
    {
        try
        {
            var command = new UpdateTenantCommand(
                id,
                request.Name,
                request.Slug,
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
                    ErrorResponseDto.NotFoundError("Tenant", HttpContext.Items["CorrelationId"]?.ToString());
                return NotFound(notFoundResponse);
            }

            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    "Failed to update tenant {TenantId}: {Errors}",
                    id,
                    string.Join(", ", result.Errors.Select(e => e.Message)));
            }

            var errorResponse = ErrorResponseDto.ValidationError(
                result.Errors.Select(e => e.Message).ToList(),
                HttpContext.Items["CorrelationId"]?.ToString());
            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating tenant {TenantId}", id);
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }

    /// <summary>
    ///     Deletes (soft delete) an existing tenant.
    ///     Requires 'tenants:delete' permission.
    /// </summary>
    /// <param name="id">The unique identifier of the tenant to delete.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.Tenants.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteTenantCommand(id);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            if (result.Errors.Any(e => e.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                var notFoundResponse =
                    ErrorResponseDto.NotFoundError("Tenant", HttpContext.Items["CorrelationId"]?.ToString());
                return NotFound(notFoundResponse);
            }

            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    "Failed to delete tenant {TenantId}: {Errors}",
                    id,
                    string.Join(", ", result.Errors.Select(e => e.Message)));
            }

            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting tenant {TenantId}", id);
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}
