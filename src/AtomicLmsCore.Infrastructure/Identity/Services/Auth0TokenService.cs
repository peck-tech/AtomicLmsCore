using System.Text.Json;
using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Infrastructure.Identity.Configuration;
using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;

namespace AtomicLmsCore.Infrastructure.Identity.Services;

/// <summary>
///     Auth0 implementation of the identity token service.
/// </summary>
public class Auth0TokenService : IIdentityTokenService
{
    private readonly Auth0Options _auth0Options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<Auth0TokenService> _logger;
    private readonly RestClient _restClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Auth0TokenService"/> class.
    /// </summary>
    /// <param name="auth0Options">The Auth0 configuration options.</param>
    /// <param name="cache">The memory cache for storing tokens.</param>
    /// <param name="logger">The logger instance.</param>
    public Auth0TokenService(
        IOptions<Auth0Options> auth0Options,
        IMemoryCache cache,
        ILogger<Auth0TokenService> logger)
    {
        _auth0Options = auth0Options.Value;
        _cache = cache;
        _logger = logger;
        _restClient = new RestClient($"https://{_auth0Options.Domain}");
    }

    /// <summary>
    ///     Gets an access token for the identity provider's management API.
    /// </summary>
    /// <returns>A result containing the access token or error information.</returns>
    public async Task<Result<string>> GetManagementTokenAsync()
    {
        return await GetTokenAsync(_auth0Options.ManagementApiAudience);
    }

    /// <summary>
    ///     Gets an access token for a specific audience.
    /// </summary>
    /// <param name="audience">The audience for the token.</param>
    /// <returns>A result containing the access token or error information.</returns>
    public async Task<Result<string>> GetTokenAsync(string audience)
    {
        try
        {
            var cacheKey = $"auth0_token_{audience}";

            if (_cache.TryGetValue<string>(cacheKey, out var cachedToken))
            {
                return Result.Ok(cachedToken!);
            }

            var request = new RestRequest("/oauth/token", Method.Post);
            request.AddHeader("content-type", "application/json");

            var body = new
            {
                client_id = _auth0Options.ClientId,
                client_secret = _auth0Options.ClientSecret,
                audience,
                grant_type = "client_credentials",
            };

            request.AddJsonBody(body);

            var response = await _restClient.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogError("Failed to get Auth0 token. Status: {StatusCode}, Content: {Content}", response.StatusCode, response.Content);
                return Result.Fail($"Failed to get Auth0 token: {response.StatusDescription}");
            }

            if (string.IsNullOrEmpty(response.Content))
            {
                return Result.Fail("Empty response from Auth0 token endpoint");
            }

            var tokenResponse = JsonSerializer.Deserialize<Auth0TokenResponse>(response.Content);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                return Result.Fail("Invalid token response from Auth0");
            }

            var expirationTime = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 60);
            _cache.Set(cacheKey, tokenResponse.AccessToken, expirationTime);

            return Result.Ok(tokenResponse.AccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Auth0 token for audience {Audience}", audience);
            return Result.Fail($"Error getting Auth0 token: {ex.Message}");
        }
    }

    private class Auth0TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }

        public string TokenType { get; set; } = string.Empty;
    }
}
