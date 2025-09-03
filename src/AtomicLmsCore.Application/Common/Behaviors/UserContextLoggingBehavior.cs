using AtomicLmsCore.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AtomicLmsCore.Application.Common.Behaviors;

/// <summary>
///     Pipeline behaviour that automatically logs user context for all MediatR requests.
///     Records whether the operation is performed by a direct user or a machine acting on behalf of a user.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <remarks>
///     Initializes a new instance of the <see cref="UserContextLoggingBehavior{TRequest, TResponse}"/> class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
/// <param name="userContextService">The user context service.</param>
public class UserContextLoggingBehavior<TRequest, TResponse>(
    ILogger<UserContextLoggingBehavior<TRequest, TResponse>> logger,
    IUserContextService userContextService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UserContextLoggingBehavior<TRequest, TResponse>> _logger = logger;
    private readonly IUserContextService _userContextService = userContextService;

    /// <summary>
    ///     Handles the pipeline behavior by logging user context before executing the request.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the next behavior in the pipeline.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information) == false)
        {
            // Continue with the pipeline
            return await next(cancellationToken);
        }

        var requestName = typeof(TRequest).Name;

        // Log user context based on authentication type
        if (_userContextService.IsMachineToMachine)
        {
            _logger.LogInformation(
                "Machine client {ClientId} executing {RequestName} on behalf of user {UserId}",
                _userContextService.AuthenticatedSubject,
                requestName,
                _userContextService.TargetUserId);
        }
        else
        {
            _logger.LogInformation(
                "User {UserId} executing {RequestName}",
                _userContextService.TargetUserId,
                requestName);
        }

        // Continue with the pipeline
        return await next(cancellationToken);
    }
}
