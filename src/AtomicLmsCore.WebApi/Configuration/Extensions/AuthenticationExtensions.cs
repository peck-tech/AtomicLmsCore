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

                            // Preserve grant type claim for determining auth flow type
                            var grantTypeClaim = claimsIdentity.Claims
                                .FirstOrDefault(c => c.Type == "gty" || c.Type.EndsWith("/gty"));
                            if (grantTypeClaim != null && grantTypeClaim.Type != "gty")
                            {
                                claimsIdentity.AddClaim(new Claim("gty", grantTypeClaim.Value));
                            }

                            // Preserve azp (authorized party) claim for machine clients
                            var azpClaim = claimsIdentity.Claims
                                .FirstOrDefault(c => c.Type == "azp" || c.Type.EndsWith("/azp"));
                            if (azpClaim != null && azpClaim.Type != "azp")
                            {
                                claimsIdentity.AddClaim(new Claim("azp", azpClaim.Value));
                            }

                            // Process scope claims for machine authentication
                            var scopeClaims = claimsIdentity.Claims
                                .Where(c => c.Type == "scope" ||
                                           c.Type.EndsWith("/scope") ||
                                           c.Type == $"{jwtOptions.Audience}/scope")
                                .ToList();

                            foreach (var claim in scopeClaims.Where(claim => claim.Type != "scope"))
                            {
                                claimsIdentity.AddClaim(new Claim("scope", claim.Value));
                            }

                            // Normalize permission claims from custom namespaces
                            var permissionClaims = claimsIdentity.Claims
                                .Where(c => c.Type.Contains("permissions", StringComparison.OrdinalIgnoreCase) ||
                                           c.Type.EndsWith("/permissions", StringComparison.OrdinalIgnoreCase) ||
                                           c.Type == $"{jwtOptions.Audience}/permissions")
                                .ToList();

                            foreach (var claim in permissionClaims.Where(claim => claim.Type != "permission"))
                            {
                                // Handle both single permissions and arrays
                                if (claim.Value.StartsWith('[') && claim.Value.EndsWith(']'))
                                {
                                    var permissions = System.Text.Json.JsonSerializer.Deserialize<string[]>(claim.Value);
                                    if (permissions != null)
                                    {
                                        foreach (var permission in permissions)
                                        {
                                            claimsIdentity.AddClaim(new Claim("permission", permission));
                                        }
                                    }
                                }
                                else
                                {
                                    claimsIdentity.AddClaim(new Claim("permission", claim.Value));
                                }
                            }

                            var customTenantClaims = claimsIdentity.Claims
                                .Where(c => c.Type.Contains("tenant_id", StringComparison.OrdinalIgnoreCase) ||
                                            c.Type.EndsWith("/tenant_id", StringComparison.OrdinalIgnoreCase) ||
                                            c.Type == $"{jwtOptions.Audience}/tenant_id")
                                .ToList();

                            foreach (var claim in customTenantClaims.Where(claim => claim.Type != "tenant_id"))
                            {
                                claimsIdentity.AddClaim(new Claim("tenant_id", claim.Value));
                            }

                            var customRoleClaims = claimsIdentity.Claims
                                .Where(c => c.Type.Contains("roles", StringComparison.OrdinalIgnoreCase) ||
                                            c.Type.EndsWith("/roles", StringComparison.OrdinalIgnoreCase) ||
                                            c.Type == $"{jwtOptions.Audience}/roles")
                                .ToList();

                            foreach (var claim in customRoleClaims.Where(claim => claim.Type != ClaimTypes.Role))
                            {
                                if (claim.Value.StartsWith('[') && claim.Value.EndsWith(']'))
                                {
                                    var roles = System.Text.Json.JsonSerializer.Deserialize<string[]>(claim.Value);
                                    if (roles == null)
                                    {
                                        continue;
                                    }

                                    foreach (var role in roles)
                                    {
                                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                                    }
                                }
                                else
                                {
                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                                }
                            }
                            return Task.CompletedTask;
                        },
                    };
                });
        }

        services
            .AddAuthorization()
            .AddPermissionAuthorization();

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
