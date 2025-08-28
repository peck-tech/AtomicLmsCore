using FluentResults;
using MediatR;

namespace AtomicLmsCore.Application.HelloWorld.Queries;

public class GetHelloWorldQuery : IRequest<Result<HelloWorldDto>>
{
    public string Name { get; set; } = string.Empty;
}

public class HelloWorldDto
{
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}