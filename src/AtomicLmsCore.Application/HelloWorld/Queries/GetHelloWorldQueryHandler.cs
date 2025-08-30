using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.HelloWorld.Queries;

public class GetHelloWorldQueryHandler(ILogger<GetHelloWorldQueryHandler> logger)
    : IRequestHandler<GetHelloWorldQuery, Result<HelloWorldDto>>
{
    public Task<Result<HelloWorldDto>> Handle(GetHelloWorldQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing Hello World request for {Name}", request.Name);

            var greeting = string.IsNullOrWhiteSpace(request.Name)
                ? "Hello World from AtomicLMS Core!"
                : $"Hello {request.Name}, welcome to AtomicLMS Core!";

            var response = new HelloWorldDto { Message = greeting, Timestamp = DateTime.UtcNow };

            return Task.FromResult(Result.Ok(response));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Hello World request");
            return Task.FromResult(Result.Fail<HelloWorldDto>("An error occurred processing your request"));
        }
    }
}
