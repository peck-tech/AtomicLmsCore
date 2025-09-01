using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AtomicLmsCore.WebApi.Configuration.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        if (jwtOptions != null)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = jwtOptions.Authority;
                    options.Audience = jwtOptions.Audience;
                    options.RequireHttpsMetadata = jwtOptions.RequireHttpsMetadata;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = jwtOptions.ValidateAudience,
                        ValidateIssuer = jwtOptions.ValidateIssuer,
                        ValidateLifetime = jwtOptions.ValidateLifetime,
                        ClockSkew = TimeSpan.FromMinutes(5),
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            if (context.Principal?.Identity is not ClaimsIdentity claimsIdentity)
                            {
                                return Task.CompletedTask;
                            }

                            var customTenantClaims = claimsIdentity.Claims
                                .Where(c => c.Type.Contains("tenant_id", StringComparison.OrdinalIgnoreCase) || c.Type.EndsWith("/tenant_id", StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            foreach (var claim in customTenantClaims.Where(claim => claim.Type != "tenant_id"))
                            {
                                claimsIdentity.AddClaim(new Claim("tenant_id", claim.Value));
                            }
                            return Task.CompletedTask;
                        },
                    };
                });
        }

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
