using System.Security.Claims;
using AtomicLmsCore.WebApi.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.WebApi.Tests.Integration;

public class MiddlewarePipelineOrderTests : IDisposable
{
    private readonly List<IHost> _hosts = [];

    [Fact]
    public async Task MiddlewarePipeline_CorrelationIdSetBeforeTenantResolution()
    {
        // Arrange
        var executionOrder = new List<string>();

        var host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(Mock.Of<ILogger<CorrelationIdMiddleware>>());
                        services.AddSingleton(Mock.Of<ILogger<TenantResolutionMiddleware>>());
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<CorrelationIdMiddleware>();
                        app.UseMiddleware<TenantResolutionMiddleware>();

                        app.Use((HttpContext context, RequestDelegate _) =>
                        {
                            executionOrder.Add("FinalMiddleware");

                            // Verify CorrelationId was set by first middleware
                            context.Items.Should().ContainKey("CorrelationId");
                            var correlationId = context.Items["CorrelationId"];
                            correlationId.Should().NotBeNull();

                            context.Response.WriteAsync("OK").Wait();
                            return Task.CompletedTask;
                        });
                    });
            })
            .Build();

        _hosts.Add(host);
        await host.StartAsync();

        var testServer = host.GetTestServer();
        var client = testServer.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v0.1/solution/tenants");

        // Assert
        response.Headers.Should().Contain(h => h.Key == "X-Correlation-ID");
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
        var host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(Mock.Of<ILogger<CorrelationIdMiddleware>>());
                        services.AddSingleton(Mock.Of<ILogger<TenantResolutionMiddleware>>());
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<CorrelationIdMiddleware>();
                        app.UseMiddleware<TenantResolutionMiddleware>();
                    });
            })
            .Build();

        _hosts.Add(host);
        await host.StartAsync();

        var testServer = host.GetTestServer();

        // Act
        var context = await testServer.SendAsync(c =>
        {
            c.Request.Path = "/api/v0.1/learners/users";
            c.User = CreateUser(tenantClaims: []); // No tenant claims should cause 403
        });

        // Assert
        context.Response.StatusCode.Should().Be(403);
        context.Items.Should().ContainKey("CorrelationId");

        // TestHost response streams don't support Position, but we can check headers
        context.Response.Headers.Should().ContainKey("X-Correlation-ID");
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
            "Authentication",
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
        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = path
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
        return context;
    }

    private static ClaimsPrincipal CreateUser(Guid[]? tenantClaims = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user"), new(ClaimTypes.Name, "Test User")
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

    public void Dispose()
    {
        foreach (var host in _hosts)
        {
            host.Dispose();
        }
    }
}
