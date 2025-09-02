using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Common.Behaviors;

[UsedImplicitly]
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
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

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

        // Check if TResponse is a generic Result<T> type
        var responseType = typeof(TResponse);
        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            return (TResponse)(object)Result.Fail(errors);
        }
        var valueType = responseType.GetGenericArguments()[0];
        // Use the specific generic Result.Fail<T> method
        var failMethod = typeof(Result)
            .GetMethods()
            .Where(m => m.Name == nameof(Result.Fail) && m.IsGenericMethodDefinition)
            .First(m => m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(IEnumerable<string>))
            .MakeGenericMethod(valueType);
        return (TResponse)failMethod.Invoke(
            null,
            [errors])!;
    }
}
