using AtomicLmsCore.Application.Common.Behaviors;
using AtomicLmsCore.Application.Tenants.Queries;
using AtomicLmsCore.WebApi.Mappings;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace AtomicLmsCore.WebApi.Configuration.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<TenantMappingProfile>();
            cfg.AddProfile<UserMappingProfile>();
        });

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(GetTenantByIdQuery).Assembly)
                .AddOpenBehavior(typeof(ValidationBehavior<,>))
                .AddOpenBehavior(typeof(TelemetryBehavior<,>));
        });

        services
            .AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssembly(typeof(GetTenantByIdQuery).Assembly);

        return services;
    }
}
