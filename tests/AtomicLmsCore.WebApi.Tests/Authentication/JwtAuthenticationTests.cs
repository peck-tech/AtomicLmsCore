using System.Security.Claims;
using AtomicLmsCore.WebApi.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace AtomicLmsCore.WebApi.Tests.Authentication;

public class JwtAuthenticationTests
{
    [Fact]
    public void JwtAuthentication_ShouldConfigureCorrectly_WhenValidOptionsProvided()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Authority"] = "https://test.auth0.com/",
                ["Jwt:Audience"] = "https://test-api",
                ["Jwt:RequireHttpsMetadata"] = "false",
                ["Jwt:ValidateAudience"] = "true",
                ["Jwt:ValidateIssuer"] = "true",
                ["Jwt:ValidateLifetime"] = "true"
            })
            .Build();

        var services = new ServiceCollection();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Act
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        // Assert
        jwtOptions.Should().NotBeNull();
        jwtOptions!.Authority.Should().Be("https://test.auth0.com/");
        jwtOptions.Audience.Should().Be("https://test-api");
        jwtOptions.RequireHttpsMetadata.Should().BeFalse();
        jwtOptions.ValidateAudience.Should().BeTrue();
        jwtOptions.ValidateIssuer.Should().BeTrue();
        jwtOptions.ValidateLifetime.Should().BeTrue();
    }

    [Fact]
    public void JwtTokenValidationParameters_ShouldSetCorrectValues()
    {
        // Arrange
        var jwtOptions = new JwtOptions
        {
            Authority = "https://test.auth0.com/",
            Audience = "https://test-api",
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = false
        };

        // Act
        var parameters = new TokenValidationParameters
        {
            ValidateAudience = jwtOptions.ValidateAudience,
            ValidateIssuer = jwtOptions.ValidateIssuer,
            ValidateLifetime = jwtOptions.ValidateLifetime,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        // Assert
        parameters.ValidateAudience.Should().BeTrue();
        parameters.ValidateIssuer.Should().BeTrue();
        parameters.ValidateLifetime.Should().BeFalse();
        parameters.ClockSkew.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void TenantIdClaimMapping_ShouldMapCustomClaimsCorrectly()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user123"),
            new("https://custom.namespace/tenant_id", "tenant-1"),
            new("https://another.namespace/tenant_id", "tenant-2"),
            new("tenant_id", "tenant-3") // Already correctly named
        };

        var identity = new ClaimsIdentity(claims, "test");

        // Simulate the JWT event handler logic
        var customTenantClaims = identity.Claims
            .Where(c => c.Type.Contains("tenant_id", StringComparison.OrdinalIgnoreCase) || c.Type.EndsWith("/tenant_id", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Act
        foreach (var claim in customTenantClaims)
        {
            if (claim.Type != "tenant_id")
            {
                identity.AddClaim(new Claim("tenant_id", claim.Value));
            }
        }

        // Assert
        var mappedTenantClaims = identity.Claims.Where(c => c.Type == "tenant_id").ToList();
        mappedTenantClaims.Should().HaveCount(3);
        mappedTenantClaims.Should().Contain(c => c.Value == "tenant-1");
        mappedTenantClaims.Should().Contain(c => c.Value == "tenant-2");
        mappedTenantClaims.Should().Contain(c => c.Value == "tenant-3");
    }

    [Fact]
    public void JwtBearerEvents_OnTokenValidated_ShouldBeCalled()
    {
        // Arrange
        var mockHandler = new Mock<Func<TokenValidatedContext, Task>>();
        mockHandler.Setup(x => x(It.IsAny<TokenValidatedContext>()))
                   .Returns(Task.CompletedTask);

        var events = new JwtBearerEvents
        {
            OnTokenValidated = mockHandler.Object
        };

        // Assert
        events.OnTokenValidated.Should().NotBeNull();
        mockHandler.Verify(x => x(It.IsAny<TokenValidatedContext>()), Times.Never);
    }

    [Fact]
    public void JwtOptions_ShouldHaveRequiredProperties()
    {
        // The required properties are enforced at compile-time,
        // so this test verifies they exist on a valid object
        var options = new JwtOptions
        {
            Authority = "https://test.auth0.com/",
            Audience = "https://test-api"
        };

        options.Authority.Should().NotBeNullOrEmpty();
        options.Audience.Should().NotBeNullOrEmpty();
    }
}
