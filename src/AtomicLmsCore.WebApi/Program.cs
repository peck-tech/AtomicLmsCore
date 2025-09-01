using AtomicLmsCore.WebApi.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureServices()
    .AddApiConfiguration()
    .AddSwaggerConfiguration()
    .AddPersistence()
    .AddApplicationServices()
    .AddJwtAuthentication(builder.Configuration)
    .AddCorsConfiguration();

var app = builder.Build();

app.ConfigureMiddleware();

app.Run();
