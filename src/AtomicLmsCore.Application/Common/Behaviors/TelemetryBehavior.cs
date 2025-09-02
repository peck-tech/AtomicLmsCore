using System.Diagnostics;
using JetBrains.Annotations;
using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Common.Behaviors;

[UsedImplicitly]
public class TelemetryBehavior<TRequest, TResponse>(
    TelemetryClient telemetryClient,
    ILogger<TelemetryBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        telemetryClient.TrackEvent(
            $"{requestName}.Started",
            new Dictionary<string, string>
            {
                {
                    "RequestType", typeof(TRequest).FullName ?? requestName
                },
                {
                    "ResponseType", typeof(TResponse).FullName ?? typeof(TResponse).Name
                },
            });

        logger.LogInformation("Processing {RequestName}", requestName);

        try
        {
            var response = await next();

            stopwatch.Stop();

            telemetryClient.TrackEvent(
                $"{requestName}.Completed",
                new Dictionary<string, string>
                {
                    {
                        "Success", "true"
                    },
                    {
                        "Duration", stopwatch.ElapsedMilliseconds.ToString()
                    },
                });

            telemetryClient.TrackMetric($"{requestName}.Duration", stopwatch.ElapsedMilliseconds);

            logger.LogInformation(
                "Completed {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            telemetryClient.TrackException(
                ex,
                new Dictionary<string, string>
                {
                    {
                        "RequestType", requestName
                    },
                    {
                        "Duration", stopwatch.ElapsedMilliseconds.ToString()
                    },
                });

            telemetryClient.TrackEvent(
                $"{requestName}.Failed",
                new Dictionary<string, string>
                {
                    {
                        "Duration", stopwatch.ElapsedMilliseconds.ToString()
                    },
                    {
                        "ExceptionType", ex.GetType().Name
                    },
                });

            logger.LogError(
                ex,
                "Error processing {RequestName} after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
