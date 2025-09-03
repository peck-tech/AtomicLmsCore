using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Infrastructure.Identity.Configuration;
using AtomicLmsCore.Infrastructure.Identity.Services;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AtomicLmsCore.Infrastructure.Tests.Identity.Auth0;

public class Auth0ManagementServiceTests
{
    private readonly Auth0ManagementService _service;
    private readonly Mock<IIdentityTokenService> _tokenServiceMock;

    public Auth0ManagementServiceTests()
    {
        var optionsMock = new Mock<IOptions<Auth0Options>>();
        _tokenServiceMock = new Mock<IIdentityTokenService>();
        var loggerMock = new Mock<ILogger<Auth0ManagementService>>();

        var auth0Options = new Auth0Options
        {
            Domain = "test.auth0.com",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            ManagementApiAudience = "https://test.auth0.com/api/v2/",
        };

        optionsMock.Setup(x => x.Value).Returns(auth0Options);

        _service = new Auth0ManagementService(optionsMock.Object, _tokenServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetUserAsync_WhenTokenServiceFails_ShouldReturnFailure()
    {
        _tokenServiceMock
            .Setup(x => x.GetManagementTokenAsync())
            .ReturnsAsync(Result.Fail("Token service error"));

        var result = await _service.GetUserAsync("user-id");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("Token service error"));
    }

    [Fact]
    public async Task CreateUserAsync_WhenTokenServiceFails_ShouldReturnFailure()
    {
        _tokenServiceMock
            .Setup(x => x.GetManagementTokenAsync())
            .ReturnsAsync(Result.Fail("Token service error"));

        var result = await _service.CreateUserAsync("test@example.com", "password123");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("Token service error"));
    }

    [Fact]
    public async Task UpdateUserMetadataAsync_WhenTokenServiceFails_ShouldReturnFailure()
    {
        _tokenServiceMock
            .Setup(x => x.GetManagementTokenAsync())
            .ReturnsAsync(Result.Fail("Token service error"));

        var result = await _service.UpdateUserMetadataAsync("user-id", new Dictionary<string, object> { ["tenantId"] = "tenant-123" });

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("Token service error"));
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WhenTokenServiceFails_ShouldReturnFailure()
    {
        _tokenServiceMock
            .Setup(x => x.GetManagementTokenAsync())
            .ReturnsAsync(Result.Fail("Token service error"));

        var result = await _service.AssignRoleToUserAsync("user-id", "role-id");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("Token service error"));
    }

    [Fact]
    public async Task GetUserRolesAsync_WhenTokenServiceFails_ShouldReturnFailure()
    {
        _tokenServiceMock
            .Setup(x => x.GetManagementTokenAsync())
            .ReturnsAsync(Result.Fail("Token service error"));

        var result = await _service.GetUserRolesAsync("user-id");

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("Token service error"));
    }
}
