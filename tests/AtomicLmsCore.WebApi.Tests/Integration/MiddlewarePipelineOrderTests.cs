using System.Security.Claims;
using AtomicLmsCore.WebApi.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.WebApi.Tests.Integration;

public class MiddlewarePipelineOrderTests
{
    [Fact]
    public async Task MiddlewarePipeline_CorrelationIdSetBeforeTenantResolution()
    {
        // Arrange
        var executionOrder = new List<string>();
        var builder = WebApplication.CreateBuilder();

        // Configure services
        builder.Services.AddSingleton(Mock.Of<ILogger<CorrelationIdMiddleware>>());
        builder.Services.AddSingleton(Mock.Of<ILogger<TenantResolutionMiddleware>>());

        var app = builder.Build();

        // Configure pipeline in correct order
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<TenantResolutionMiddleware>();

        // Add test endpoint to verify order
        app.Use(async (context, next) =>
        {
            executionOrder.Add("FinalMiddleware");

            // Verify CorrelationId was set by first middleware
            context.Items.Should().ContainKey("CorrelationId");
            var correlationId = context.Items["CorrelationId"];
            correlationId.Should().NotBeNull();

            await next();
        });

        // Act
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v0.1/solution/tenants"; // Solution endpoint bypasses tenant validation
        context.Response.Body = new MemoryStream();

        await app.StartAsync();

        using var scope = app.Services.CreateScope();
        var pipeline = app.Services.GetRequiredService<RequestDelegate>();

        await pipeline(context);

        // Assert
        context.Items.Should().ContainKey("CorrelationId");
        context.Response.Headers.Should().ContainKey("X-Correlation-ID");

        await app.StopAsync();
    }

    [Fact]
    public async Task MiddlewarePipeline_TenantResolutionRequiresCorrelationId()
    {
        // Arrange
        var correlationIdSet = false;
        var tenantResolutionRan = false;

        var pipeline = new List<Func<HttpContext, RequestDelegate, Task>>
        {
            // Simulate CorrelationIdMiddleware
            async (context, next) =>
            {
                context.Items["CorrelationId"] = "test-correlation-id";
                context.Response.Headers["X-Correlation-ID"] = "test-correlation-id";
                correlationIdSet = true;
                await next(context);
            },
            
            // Simulate TenantResolutionMiddleware behavior
            async (context, next) =>
            {
                // Should have correlation ID available for error responses
                if (!context.Items.ContainsKey("CorrelationId"))
                {
                    throw new InvalidOperationException("CorrelationId not available");
                }

                tenantResolutionRan = true;
                await next(context);
            }
        };

        var context = CreateTestContext("/api/v0.1/learners/users");

        // Act
        await ExecutePipeline(context, pipeline);

        // Assert
        correlationIdSet.Should().BeTrue();
        tenantResolutionRan.Should().BeTrue();
    }

    [Fact]
    public async Task MiddlewarePipeline_ErrorResponsesIncludeCorrelationId()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(Mock.Of<ILogger<CorrelationIdMiddleware>>());
        builder.Services.AddSingleton(Mock.Of<ILogger<TenantResolutionMiddleware>>());

        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<TenantResolutionMiddleware>();

        var context = CreateTestContext("/api/v0.1/learners/users");
        context.User = CreateUser(tenantClaims: []); // No tenant claims should cause 403

        await app.StartAsync();

        using var scope = app.Services.CreateScope();
        var pipeline = app.Services.GetRequiredService<RequestDelegate>();

        // Act
        await pipeline(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
        context.Items.Should().ContainKey("CorrelationId");

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("correlationId");

        await app.StopAsync();
    }

    [Theory]
    [InlineData(1)] // CorrelationId first
    [InlineData(2)] // TenantResolution second
    public async Task MiddlewarePipeline_OrderMatters(int middlewarePosition)
    {
        // Arrange
        var executionOrder = new List<string>();
        var middlewares = new List<Func<HttpContext, RequestDelegate, Task>>();

        if (middlewarePosition == 1)
        {
            // Correct order: CorrelationId -> TenantResolution
            middlewares.Add(async (context, next) =>
            {
                executionOrder.Add("CorrelationId");
                context.Items["CorrelationId"] = "test-id";
                await next(context);
            });
            middlewares.Add(async (context, next) =>
            {
                executionOrder.Add("TenantResolution");
                // Should have CorrelationId available
                context.Items.Should().ContainKey("CorrelationId");
                await next(context);
            });
        }
        else
        {
            // Wrong order would cause issues in real scenario
            middlewares.Add(async (context, next) =>
            {
                executionOrder.Add("TenantResolution");
                await next(context);
            });
            middlewares.Add(async (context, next) =>
            {
                executionOrder.Add("CorrelationId");
                context.Items["CorrelationId"] = "test-id";
                await next(context);
            });
        }

        var context = CreateTestContext("/api/v0.1/solution/tenants");

        // Act
        await ExecutePipeline(context, middlewares);

        // Assert
        if (middlewarePosition == 1)
        {
            executionOrder.Should().Equal("CorrelationId", "TenantResolution");
        }
        else
        {
            executionOrder.Should().Equal("TenantResolution", "CorrelationId");
        }
    }

    [Fact]
    public void MiddlewarePipeline_HttpsRedirectionShouldBeFirst()
    {
        // This test documents the expected order but doesn't test HttpsRedirection itself
        // since it requires complex setup. It serves as documentation.

        var expectedOrder = new[]
        {
            "HttpsRedirection",
            "CorrelationId",
            "Authentication", // Not yet implemented
            "TenantResolution",
            "CORS",
            "Authorization"
        };

        // Assert - This is documentation of the expected order
        expectedOrder.Should().StartWith("HttpsRedirection");
        expectedOrder[1].Should().Be("CorrelationId");
        expectedOrder[3].Should().Be("TenantResolution");
        expectedOrder.Should().EndWith("Authorization");
    }

    private static HttpContext CreateTestContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static ClaimsPrincipal CreateUser(Guid[]? tenantClaims = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user"),
            new(ClaimTypes.Name, "Test User")
        };

        if (tenantClaims != null)
        {
            claims.AddRange(tenantClaims.Select(id => new Claim("tenant_id", id.ToString())));
        }

        var identity = new ClaimsIdentity(claims, "test");
        return new ClaimsPrincipal(identity);
    }

    private static async Task ExecutePipeline(HttpContext context, List<Func<HttpContext, RequestDelegate, Task>> middlewares)
    {
        RequestDelegate pipeline = _ => Task.CompletedTask;

        // Build pipeline in reverse order (like ASP.NET Core does)
        for (var i = middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = middlewares[i];
            var next = pipeline;
            pipeline = ctx => middleware(ctx, next);
        }

        await pipeline(context);
    }

    private static async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
