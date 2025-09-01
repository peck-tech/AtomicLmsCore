using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace AtomicLmsCore.WebApi.Configuration.Extensions;

public static class ApiConfigurationExtensions
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddApiVersioning(opt =>
        {
            opt.DefaultApiVersion = new ApiVersion(0, 1);
            opt.AssumeDefaultVersionWhenUnspecified = true;
        });

        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            var openApiInfo = new OpenApiInfo
            {
                Title = "AtomicLMS Core API",
                Version = "v0.1",
                Description = "A headless LMS API designed to be versatile and simple",
            };
            c.SwaggerDoc("v0.1", openApiInfo);

            c.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                });

            c.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        []
                    },
                });
        });

        return services;
    }
}
