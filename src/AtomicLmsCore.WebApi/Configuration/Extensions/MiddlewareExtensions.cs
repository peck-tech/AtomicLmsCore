using AtomicLmsCore.WebApi.Authorization;
using AtomicLmsCore.WebApi.Middleware;

namespace AtomicLmsCore.WebApi.Configuration.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app
                .UseSwagger()
                .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v0.1/swagger.json", "AtomicLMS Core API V0.1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection()
            .UseMiddleware<CorrelationIdMiddleware>()
            .UseMiddleware<HealthCheckAuthenticationMiddleware>()
            .UseAuthentication()
            .UseMiddleware<UserResolutionMiddleware>()
            .UseMiddleware<TenantResolutionMiddleware>()
            .UseCors()
            .UseAuthorization()
            .UseMiddleware<PermissionAuthorizationMiddleware>();
        app.MapControllers();

        return app;
    }
}
