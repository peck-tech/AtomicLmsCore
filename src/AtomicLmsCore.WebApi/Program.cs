using AtomicLmsCore.WebApi.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureServices()
    .AddIdentityProviderServices(builder.Configuration)
    .AddApiConfiguration()
    .AddSwaggerConfiguration()
    .AddPersistence()
    .AddApplicationServices()
    .AddJwtAuthentication(builder.Configuration)
    .AddCorsConfiguration()
    .AddHealthCheckConfiguration(builder.Configuration);

var app = builder.Build();

app.ConfigureMiddleware()
    .MapHealthCheckEndpoints();

app.Run();

// Make Program class accessible for testing
public partial class Program
{
}
