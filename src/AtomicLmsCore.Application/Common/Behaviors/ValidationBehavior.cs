using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResultBase
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures.Select(failure => failure.ErrorMessage).ToList();
        var requestName = typeof(TRequest).Name;

        logger.LogWarning(
            "Validation failed for {RequestName}: {ValidationErrors}",
            requestName,
            string.Join(", ", errors));

        return (TResponse)(object)Result.Fail(errors);
    }
}
