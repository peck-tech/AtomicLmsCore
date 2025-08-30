using AtomicLmsCore.Application.HelloWorld.Queries;
using AtomicLmsCore.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AtomicLmsCore.WebApi.Controllers;

[ApiController]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HelloWorldController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HelloWorldController> _logger;

    public HelloWorldController(IMediator mediator, ILogger<HelloWorldController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Returns a Hello World greeting.
    /// </summary>
    /// <param name="name">Optional name for personalized greeting.</param>
    /// <returns>HelloWorldDto with greeting message.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(HelloWorldDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get([FromQuery] string? name = null)
    {
        try
        {
            var query = new GetHelloWorldQuery { Name = name ?? string.Empty };
            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            _logger.LogWarning(
                "Failed to process Hello World request: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Message)));

            var errorResponse = ErrorResponseDto.ValidationError(
                result.Errors.Select(e => e.Message).ToList(),
                HttpContext.Items["CorrelationId"]?.ToString());
            return BadRequest(errorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Hello World request");
            var errorResponse = ErrorResponseDto.SystemError(HttpContext.Items["CorrelationId"]?.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
        }
    }
}
