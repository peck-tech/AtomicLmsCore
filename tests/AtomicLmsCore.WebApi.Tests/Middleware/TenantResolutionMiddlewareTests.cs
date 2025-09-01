using System.Security.Claims;
using AtomicLmsCore.WebApi.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.WebApi.Tests.Middleware;

public class TenantResolutionMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<TenantResolutionMiddleware>> _loggerMock;
    private readonly TenantResolutionMiddleware _middleware;

    public TenantResolutionMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<TenantResolutionMiddleware>>();
        _middleware = new TenantResolutionMiddleware(_nextMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("/api/v0.1/solution/tenants")]
    [InlineData("/api/v1.0/solution/users")]
    public async Task InvokeAsync_SolutionFeatureBucketRequest_BypassesMiddleware(string path)
    {
        // Arrange
        var context = CreateHttpContext(path);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
        context.Items.Should().NotContainKey("ValidatedTenantId");
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/swagger")]
    [InlineData("/")]
    public async Task InvokeAsync_NonApiRequest_BypassesMiddleware(string path)
    {
        // Arrange
        var context = CreateHttpContext(path);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
        context.Items.Should().NotContainKey("ValidatedTenantId");
    }

    [Fact]
    public async Task InvokeAsync_UnauthenticatedRequest_BypassesMiddleware()
    {
        // Arrange
        var context = CreateHttpContext("/api/v0.1/learners/users");
        context.User = new ClaimsPrincipal(); // Not authenticated

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
        context.Items.Should().NotContainKey("ValidatedTenantId");
    }

    [Fact]
    public async Task InvokeAsync_SuperAdminWithValidHeader_Success()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var context = CreateHttpContext("/api/v0.1/learners/users");
        context.Request.Headers["X-Tenant-Id"] = tenantId.ToString();
        context.User = CreateUser(roles: ["superadmin"]);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
        context.Items["ValidatedTenantId"].Should().Be(tenantId);
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_SuperAdminWithInvalidHeader_ReturnsBadRequest()
    {
        // Arrange
        var context = CreateHttpContext("/api/v0.1/learners/users");
        context.Request.Headers["X-Tenant-Id"] = "invalid-guid";
        context.User = CreateUser(roles: ["superadmin"]);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Never);
        context.Response.StatusCode.Should().Be(400);

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("SuperAdmin must provide X-Tenant-Id header");
    }

    [Fact]
    public async Task InvokeAsync_SuperAdminWithNoHeader_ReturnsBadRequest()
    {
        // Arrange
        var context = CreateHttpContext("/api/v0.1/learners/users");
        context.User = CreateUser(roles: ["superadmin"]);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Never);
        context.Response.StatusCode.Should().Be(400);

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("SuperAdmin must provide X-Tenant-Id header");
    }

    [Fact]
    public async Task InvokeAsync_SingleTenantClaimNoHeader_AutoResolveSuccess()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var context = CreateHttpContext("/api/v0.1/learners/users");
        context.User = CreateUser(tenantClaims: [tenantId]);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
        context.Items["ValidatedTenantId"].Should().Be(tenantId);
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_SingleTenantClaimMatchingHeader_Success()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var context = CreateHttpContext("/api/v0.1/learners/users");
        context.Request.Headers["X-Tenant-Id"] = tenantId.ToString();
        context.User = CreateUser(tenantClaims: [tenantId]);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
        context.Items["ValidatedTenantId"].Should().Be(tenantId);
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_SingleTenantClaimNonMatchingHeader_ReturnsForbidden()
    {
        // Arrange
        var userTenantId = Guid.NewGuid();
        var headerTenantId = Guid.NewGuid();
        var context = CreateHttpContext("/api/v0.1/learners/users");
        context.Request.Headers["X-Tenant-Id"] = headerTenantId.ToString();
        context.User = CreateUser(tenantClaims: [userTenantId]);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Never);
        context.Response.StatusCode.Should().Be(403);

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("does not match user").And.Contain("authorized tenant");
    }

    [Fact]
    public async Task InvokeAsync_MultipleTenantClaimsHeaderMatchesOne_Success()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        var context = CreateHttpContext("/api/v0.1/learners/users");
        context.Request.Headers["X-Tenant-Id"] = tenant2Id.ToString();
        context.User = CreateUser(tenantClaims: [tenant1Id, tenant2Id]);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
        context.Items["ValidatedTenantId"].Should().Be(tenant2Id);
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_MultipleTenantClaimsNoHeader_ReturnsBadRequest()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        var context = CreateHttpContext("/api/v0.1/learners/users");
        context.User = CreateUser(tenantClaims: [tenant1Id, tenant2Id]);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Never);
        context.Response.StatusCode.Should().Be(400);

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("User has access to multiple tenants");
        responseBody.Should().Contain("X-Tenant-Id header is required");
        responseBody.Should().Contain(tenant1Id.ToString());
        responseBody.Should().Contain(tenant2Id.ToString());
    }

    [Fact]
    public async Task InvokeAsync_MultipleTenantClaimsHeaderMatchesNone_ReturnsForbidden()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        var unauthorizedTenantId = Guid.NewGuid();
        var context = CreateHttpContext("/api/v0.1/learners/users");
        context.Request.Headers["X-Tenant-Id"] = unauthorizedTenantId.ToString();
        context.User = CreateUser(tenantClaims: [tenant1Id, tenant2Id]);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Never);
        context.Response.StatusCode.Should().Be(403);

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("does not match any of user").And.Contain("authorized tenants");
    }

    [Fact]
    public async Task InvokeAsync_NoTenantClaims_ReturnsForbidden()
    {
        // Arrange
        var context = CreateHttpContext("/api/v0.1/learners/users");
        context.User = CreateUser(tenantClaims: []);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Never);
        context.Response.StatusCode.Should().Be(403);

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("User has no tenant claims");
        responseBody.Should().Contain("Access restricted to solution endpoints only");
    }

    [Theory]
    [InlineData("/api/v0.1/administration/settings")]
    [InlineData("/api/v0.1/learning/courses")]
    [InlineData("/api/v0.1/learners/users")]
    [InlineData("/api/v0.1/engagement/activities")]
    public async Task InvokeAsync_AllNonSolutionEndpoints_RequireTenantResolution(string path)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var context = CreateHttpContext(path);
        context.User = CreateUser(tenantClaims: [tenantId]);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(x => x(context), Times.Once);
        context.Items["ValidatedTenantId"].Should().Be(tenantId);
    }

    private static HttpContext CreateHttpContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        context.Items["CorrelationId"] = Guid.NewGuid().ToString();
        return context;
    }

    private static ClaimsPrincipal CreateUser(string[]? roles = null, Guid[]? tenantClaims = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Name, "Test User")
        };

        if (roles != null)
        {
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        if (tenantClaims != null)
        {
            claims.AddRange(tenantClaims.Select(tenantId => new Claim("tenant_id", tenantId.ToString())));
        }

        var identity = new ClaimsIdentity(claims, "test");
        return new ClaimsPrincipal(identity);
    }

    private static async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
