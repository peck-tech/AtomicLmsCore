using AtomicLmsCore.Application.Common.Behaviors;
using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Tenants.Queries;
using AtomicLmsCore.Application.Tenants.Services;
using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Persistence;
using AtomicLmsCore.Infrastructure.Persistence.Repositories;
using AtomicLmsCore.Infrastructure.Services;
using AtomicLmsCore.WebApi.Mappings;
using AtomicLmsCore.WebApi.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new(0, 1);
    opt.AssumeDefaultVersionWhenUnspecified = true;
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(c =>
{
    var openApiInfo = new OpenApiInfo
    {
        Title = "AtomicLMS Core API",
        Version = "v0.1",
        Description = "A headless LMS API designed to be versatile and simple",
    };
    c.SwaggerDoc("v0.1", openApiInfo);
});

// Register core services
builder.Services.AddScoped<IIdGenerator, UlidIdGenerator>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantAccessor, TenantAccessor>();
builder.Services.AddScoped<IConnectionStringProvider, ConnectionStringProvider>();
builder.Services.AddScoped<IDatabaseOperations, SqlServerDatabaseOperations>();
builder.Services.AddScoped<ITenantDatabaseValidator, TenantDatabaseValidator>();
builder.Services.AddScoped<ITenantDatabaseService, TenantDatabaseService>();

// Register Solutions database context and repositories
builder.Services.AddScoped(serviceProvider =>
{
    var connectionStringProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
    var connectionString = connectionStringProvider.GetSolutionsConnectionString();

    var options = new DbContextOptionsBuilder<SolutionsDbContext>()
        .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(SolutionsDbContext).Assembly.FullName))
        .Options;

    return new SolutionsDbContext(options);
});

builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ITenantService, TenantService>();

// Register Tenant database context and repositories
builder.Services.AddScoped(serviceProvider =>
{
    var tenantAccessor = serviceProvider.GetRequiredService<ITenantAccessor>();
    var connectionStringProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();
    var tenantRepository = serviceProvider.GetRequiredService<ITenantRepository>();
    var idGenerator = serviceProvider.GetRequiredService<IIdGenerator>();

    var currentTenantId = tenantAccessor.GetCurrentTenantId();
    if (!currentTenantId.HasValue)
    {
        // Return a dummy context for DI registration validation
        var dummyOptions = new DbContextOptionsBuilder<TenantDbContext>()
            .UseInMemoryDatabase("DummyTenant")
            .Options;
        return new(dummyOptions, idGenerator);
    }

    // Look up the tenant to get the database name
    var tenant = tenantRepository.GetByIdAsync(currentTenantId.Value).GetAwaiter().GetResult();
    if (tenant == null || string.IsNullOrEmpty(tenant.DatabaseName))
    {
        // Return a dummy context if tenant not found or no database name
        var dummyOptions = new DbContextOptionsBuilder<TenantDbContext>()
            .UseInMemoryDatabase("DummyTenant")
            .Options;
        return new(dummyOptions, idGenerator);
    }

    var connectionString = connectionStringProvider.GetTenantConnectionString(tenant.DatabaseName);
    var options = new DbContextOptionsBuilder<TenantDbContext>()
        .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(TenantDbContext).Assembly.FullName))
        .Options;

    return new TenantDbContext(options, idGenerator);
});

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<TenantMappingProfile>();
    cfg.AddProfile<UserMappingProfile>();
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetTenantByIdQuery).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(TelemetryBehavior<,>));
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssembly(typeof(GetTenantByIdQuery).Assembly);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v0.1/swagger.json", "AtomicLMS Core API V0.1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
