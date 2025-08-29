using AtomicLmsCore.Application.HelloWorld.Queries;
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Hello World request");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "An error occurred processing your request" });
        }
    }
}
