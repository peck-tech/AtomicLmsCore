using AtomicLmsCore.WebApi.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AtomicLmsCore.WebApi.Tests.Configuration;

public class JwtOptionsTests
{
    [Fact]
    public void JwtOptions_ShouldHaveCorrectSectionName()
    {
        JwtOptions.SectionName.Should().Be("Jwt");
    }

    [Fact]
    public void JwtOptions_ShouldBindFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Authority"] = "https://test.auth0.com/",
                ["Jwt:Audience"] = "https://test-api",
                ["Jwt:RequireHttpsMetadata"] = "false",
                ["Jwt:ValidateAudience"] = "true",
                ["Jwt:ValidateIssuer"] = "true",
                ["Jwt:ValidateLifetime"] = "false"
            })
            .Build();

        var services = new ServiceCollection();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<JwtOptions>>();

        options.Value.Authority.Should().Be("https://test.auth0.com/");
        options.Value.Audience.Should().Be("https://test-api");
        options.Value.RequireHttpsMetadata.Should().BeFalse();
        options.Value.ValidateAudience.Should().BeTrue();
        options.Value.ValidateIssuer.Should().BeTrue();
        options.Value.ValidateLifetime.Should().BeFalse();
    }

    [Fact]
    public void JwtOptions_ShouldHaveDefaultValues()
    {
        var options = new JwtOptions
        {
            Authority = "https://test.auth0.com/",
            Audience = "https://test-api"
        };

        options.RequireHttpsMetadata.Should().BeTrue();
        options.ValidateAudience.Should().BeTrue();
        options.ValidateIssuer.Should().BeTrue();
        options.ValidateLifetime.Should().BeTrue();
    }
}
