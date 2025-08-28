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

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB or full instance)

### Running the Application

1. Clone the repository
2. Navigate to the project root
3. Build the solution:
   ```bash
   dotnet build
   ```
4. Run the Web API:
   ```bash
   cd src/AtomicLmsCore.WebApi
   dotnet run
   ```
5. Open browser to view Swagger UI at: http://localhost:5000 (or https://localhost:5001)

### Test the Hello World Endpoint

```bash
curl http://localhost:5000/api/HelloWorld
curl "http://localhost:5000/api/HelloWorld?name=John"
```

## Development Guidelines

See [docs/CodingStandards.md](docs/CodingStandards.md) for detailed coding standards and conventions.