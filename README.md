# AtomicLMS Core

A headless Learning Management System (LMS) API designed to be versatile and simple.

## Architecture

Clean Architecture with the following projects:
- **AtomicLmsCore.Domain** - Core business entities and interfaces
- **AtomicLmsCore.Application** - Business logic, CQRS handlers, DTOs
- **AtomicLmsCore.Infrastructure** - Data access, external services
- **AtomicLmsCore.WebApi** - RESTful API with Swagger documentation

## Tech Stack

- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- MediatR (CQRS pattern)
- FluentValidation
- FluentResults
- AutoMapper
- Swagger/OpenAPI
- Multi-tenant architecture with database-per-tenant isolation
- JWT Bearer authentication via Auth0

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server LocalDB (recommended) or SQL Server Express/Standard
  - LocalDB is included with Visual Studio or can be installed separately
  - Verify installation: `sqllocaldb info`

### Configuration Setup

1. **Database Configuration**: Update connection strings in `src/AtomicLmsCore.WebApi/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "SolutionsDatabase": "Server=(localdb)\\mssqllocaldb;Database=AtomicLms_Solutions;Trusted_Connection=True;MultipleActiveResultSets=true",
       "TenantDatabaseTemplate": "Server=(localdb)\\mssqllocaldb;Database={DatabaseName};Trusted_Connection=True;MultipleActiveResultSets=true",
       "MasterDatabase": "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

2. **JWT Configuration**: Configure Auth0 settings in `appsettings.json`:
   ```json
   {
     "Jwt": {
       "Authority": "https://your-auth0-domain.auth0.com/",
       "Audience": "https://your-api-audience"
     }
   }
   ```

3. **Tenant Validation Secret**: Add to user secrets or appsettings:
   ```bash
   dotnet user-secrets set "TenantValidation:Secret" "your-secure-secret-key"
   ```

4. **Health Check Configuration**: Configure health check authentication:
   ```bash
   dotnet user-secrets set "HealthCheck:Secret" "your-health-check-secret-key"
   ```

### Running the Application

1. Clone the repository
2. Navigate to the project root
3. Build the solution:
   ```bash
   dotnet build
   ```
4. Run database migrations (will create Solutions database):
   ```bash
   cd src/AtomicLmsCore.WebApi
   dotnet ef database update --context SolutionsDbContext
   ```
5. Run the Web API:
   ```bash
   dotnet run
   ```
6. Open browser to view Swagger UI at: https://localhost:7001

## Multi-Tenant Architecture

AtomicLMS uses a **database-per-tenant** architecture for complete data isolation:

- **Solutions Database**: Stores tenant definitions and superadmin data
- **Tenant Databases**: Each tenant has their own database instance
- **Feature Buckets**: API organized into logical feature areas:
  - `api/v0.1/solution/*` - Tenant management (superadmin only)
  - `api/v0.1/learning/*` - Learning content management
  - `api/v0.1/learners/*` - User management
  - `api/v0.1/administration/*` - Tenant configuration
  - `api/v0.1/engagement/*` - Learner interactions

### Tenant Resolution

- **Single tenant access**: No `X-Tenant-Id` header needed
- **Multi-tenant access**: Include `X-Tenant-Id: {tenant-guid}` header
- **SuperAdmin**: Must specify `X-Tenant-Id` for tenant-specific operations

### Security Features

- Cryptographic tenant database validation
- JWT Bearer authentication via Auth0
- Role-based authorization (superadmin, admin, user)
- Automatic tenant boundary enforcement

## API Testing

See [docs/APITesting.md](docs/APITesting.md) for comprehensive API testing documentation including:
- Postman collection usage and import instructions
- Programmatic collection regeneration from integration tests
- Newman CLI automation for CI/CD pipelines

## Testing

The solution includes comprehensive testing at multiple levels:

### Unit Tests
Run unit tests across all layers:
```bash
dotnet test --filter Category=Unit
```

### Integration Tests
Complete API integration testing with WebApplicationFactory:
```bash
dotnet test --project tests/AtomicLmsCore.IntegrationTests
```

Integration tests cover:
- Authentication and authorization scenarios
- Multi-tenant data isolation
- CRUD operations for all entities
- Validation and error handling
- Postman collection generation

### Test Coverage
Tests use:
- **xUnit** - Test framework
- **FluentAssertions** - Readable assertions
- **Moq** - Mocking dependencies
- **WebApplicationFactory** - Integration testing

## Development Workflow

### Code Standards
1. Follow coding standards (see [Documentation](#documentation) section)
2. Run before committing:
   ```bash
   dotnet format && dotnet build && dotnet test
   ```
3. All commands must pass without warnings or errors

### Adding New Features
1. **API Endpoints**: Create integration tests (mandatory)
2. **Business Logic**: Add unit tests with high coverage
3. **Database Changes**: Use EF Core migrations
4. **Documentation**: Update architectural decisions if needed

## Health Checks

AtomicLMS provides comprehensive health monitoring with two-tier security:

### Public Endpoints (No Authentication)
- `GET /health` - Basic health status
- `GET /health/live` - Liveness probe for load balancers

### Protected Endpoints (Require Custom Header)
- `GET /health/detailed` - Full diagnostic information with timing
- `GET /health/ready` - Critical services readiness check

### Usage Examples

**Basic Health Check:**
```bash
curl https://localhost:7001/health
# Returns: Healthy
```

**Detailed Health Check:**
```bash
curl -H "X-Health-Check-Key: your-secret-key" https://localhost:7001/health/detailed
# Returns detailed JSON with service status, timing, and diagnostics
```

**Ready Check (for orchestration):**
```bash
curl -H "X-Health-Check-Key: your-secret-key" https://localhost:7001/health/ready
# Returns critical services status only
```

### Configuration
Configure the health check secret via user secrets or appsettings:
```bash
dotnet user-secrets set "HealthCheck:Secret" "your-health-check-secret-key"
```

## Troubleshooting

### Common Issues

**Database Connection Errors**:
- Verify LocalDB is running: `sqllocaldb start MSSQLLocalDB`
- Check connection strings in `appsettings.Development.json`
- Ensure Solutions database exists: `dotnet ef database update --context SolutionsDbContext`

**Authentication Issues**:
- Verify Auth0 configuration in `appsettings.json`
- Check JWT token format in Swagger UI
- Confirm user roles and tenant claims in token

**Multi-Tenant Issues**:
- Include `X-Tenant-Id` header for multi-tenant users
- Verify tenant exists in Solutions database
- Check tenant database provisioning logs

**Health Check Issues**:
- Verify health check secret is configured
- Check that `X-Health-Check-Key` header matches configured secret
- Confirm Auth0 connectivity for detailed diagnostics

## Documentation

### Development Guidelines
- **[Coding Standards](docs/CodingStandards.md)** - Code organization, C#/.NET standards, API design patterns
- **[Architectural Decisions](docs/ArchitecturalDecisions.md)** - Multi-tenancy strategy, database design, authentication choices
- **[Unit Test Policy](docs/UnitTestPolicy.md)** - Testing requirements, coverage standards, test structure guidelines
- **[Identifier Usage](docs/IdentifierUsageExample.md)** - Hybrid ID approach examples and implementation patterns

### API Testing
- **[API Testing Guide](docs/APITesting.md)** - Postman collections, integration test automation, Newman CLI usage