using AtomicLmsCore.Infrastructure.Identity.Configuration;
using AtomicLmsCore.Infrastructure.Identity.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AtomicLmsCore.Infrastructure.Tests.Identity.Auth0;

public class Auth0TokenServiceTests : IDisposable
{
    private readonly Auth0TokenService _service;
    private readonly IMemoryCache _cache;

    public Auth0TokenServiceTests()
    {
        var optionsMock = new Mock<IOptions<Auth0Options>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        var loggerMock = new Mock<ILogger<Auth0TokenService>>();

        var auth0Options = new Auth0Options
        {
            Domain = "test.auth0.com",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            ManagementApiAudience = "https://test.auth0.com/api/v2/",
        };

        optionsMock.Setup(x => x.Value).Returns(auth0Options);

        _service = new Auth0TokenService(optionsMock.Object, _cache, loggerMock.Object);
    }

    [Fact]
    public async Task GetManagementTokenAsync_ShouldUseManagementApiAudience()
    {
        var expectedAudience = "https://test.auth0.com/api/v2/";
        var cacheKey = $"auth0_token_{expectedAudience}";
        var expectedToken = "management-api-token";

        _cache.Set(cacheKey, expectedToken);

        var result = await _service.GetManagementTokenAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedToken);
    }

    [Fact]
    public async Task GetTokenAsync_WhenTokenInCache_ShouldReturnCachedToken()
    {
        var audience = "https://test-audience";
        var cacheKey = $"auth0_token_{audience}";
        var cachedToken = "cached-token";

        _cache.Set(cacheKey, cachedToken);

        var result = await _service.GetTokenAsync(audience);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(cachedToken);
    }

    public void Dispose()
    {
        _cache?.Dispose();
    }
}
