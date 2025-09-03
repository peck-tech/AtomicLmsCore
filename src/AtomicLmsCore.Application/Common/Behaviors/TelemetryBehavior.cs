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

        // Only perform telemetry operations if telemetry is enabled
        var telemetryEnabled = telemetryClient.IsEnabled();
        if (telemetryEnabled)
        {
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
        }

        // Only log if Information level logging is enabled
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Processing {RequestName}", requestName);
        }

        try
        {
            var response = await next(cancellationToken);

            stopwatch.Stop();

            if (telemetryEnabled)
            {
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
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Completed {RequestName} in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            if (telemetryEnabled)
            {
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
            }

            // Always log errors regardless of level (Error level should typically be enabled)
            logger.LogError(
                ex,
                "Error processing {RequestName} after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
