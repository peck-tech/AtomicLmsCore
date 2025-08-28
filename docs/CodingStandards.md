# AtomicLMS Core Coding Standards

## Overview
Coding standards for AtomicLMS Core, a headless LMS designed to be versatile and simple.

## Code Organization

### Architecture Patterns
- Use CQRS with MediatR
- Push logic to domain models rather than services
- Use a service layer extensively
- App is multi-tenant intended to be SAAS

### CQRS-Specific
- Commands should return only IDs, not full objects
- Queries should bypass domain logic entirely
- Event sourcing for critical student progress tracking

### Multi-Tenant Specific
- Tenant context must be resolved before authorization
- Cross-tenant should not be possible at this time
- Connection string per tenant pattern for data isolation

### Project Structure
- Group related functionality together by feature

### C# Standards
- Support a Fluent coding style where possible by returning this instead of void
- Mutate objects through predicates

### ASP.NET Core Standards
- Use as much config as possible, provided via IOptions pattern
- Use Automapper to map between Entities, Domain Models, and DTOs
- Use IHostedService for background tasks

## API Design
- The web API should be RESTful
- Use DTOs for request and response data
- Document API controllers and methods with OpenAPI
- API methods should catch exceptions
- Exceptions should not be used to control code flow
- Caught exceptions should be logged to ILogger

## Domain-Specific Standards
- Tenant isolation validation at repository level
- Soft deletes by default for all student/course data
- Immutable audit logs for all grade/enrollment changes

## Database Conventions

### Entity Framework Core Standards
- Target SQL Server as primary database
- Use code-first migrations
- Configure entities using Fluent API over data annotations
- Use DbContext pooling for performance
- Use async methods for all database operations
- Enable query splitting for complex includes
- Use compiled queries for frequently executed queries
- Use temporal tables for audit trails

### Naming
- Use singular table names (Course, not Courses)
- Use PascalCase for table and column names
- Prefix boolean columns with Is/Has/Can

## Service Layer Standards
- Use FluentResult for all service responses
- Catch exceptions in the service layer and return suitable errors within a FluentResult
- Use FluentValidation

## Performance Standards
- Batch database operations in background jobs

## Event-Driven Architecture
- Events should be published for all user actions
- Events should be published for all database write operations
- Use MediatR for event publishing

## Testing Standards
- Add unit tests where possible to maximise code coverage

## Development Workflow
- When making code changes, the project must be formatted, built, and tests run

## LMS-Specific Conventions
- Enrollment state machines should be explicit
- Grade calculations must be idempotent
- Progress tracking should be eventually consistent

## Security Standards
- Follow OWASP security guidelines