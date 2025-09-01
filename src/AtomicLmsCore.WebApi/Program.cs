using System.Security.Claims;
using AtomicLmsCore.Application.Common.Behaviors;
using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Tenants.Queries;
using AtomicLmsCore.Application.Tenants.Services;
using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Persistence;
using AtomicLmsCore.Infrastructure.Persistence.Repositories;
using AtomicLmsCore.Infrastructure.Services;
using AtomicLmsCore.WebApi.Configuration;
using AtomicLmsCore.WebApi.Mappings;
using AtomicLmsCore.WebApi.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(0, 1);
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

    // Add JWT Bearer authentication to Swagger
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
                Array.Empty<string>()
            },
        });
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

    // Middleware guarantees valid tenant ID exists
    var currentTenantId = tenantAccessor.GetRequiredCurrentTenantId();

    // Look up the tenant to get the database name
    var tenant = tenantRepository.GetByIdAsync(currentTenantId).GetAwaiter().GetResult();
    if (tenant == null || string.IsNullOrEmpty(tenant.DatabaseName))
    {
        throw new InvalidOperationException($"Tenant {currentTenantId} not found or missing database name");
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

// Configure JWT authentication
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

if (jwtOptions != null)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                    // Ensure tenant_id claims are properly mapped
                    var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                    if (claimsIdentity != null)
                    {
                        // Auth0 might send tenant_id in custom namespace, map it to our expected claim type
                        var customTenantClaims = claimsIdentity.Claims
                            .Where(c => c.Type.Contains("tenant_id", StringComparison.OrdinalIgnoreCase) || c.Type.EndsWith("/tenant_id", StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        foreach (var claim in customTenantClaims)
                        {
                            if (claim.Type != "tenant_id")
                            {
                                claimsIdentity.AddClaim(new Claim("tenant_id", claim.Value));
                            }
                        }
                    }
                    return Task.CompletedTask;
                },
            };
        });
}

builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
