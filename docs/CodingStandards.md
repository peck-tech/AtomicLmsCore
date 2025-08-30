# AtomicLMS Core Coding Standards

## Overview
Coding standards for AtomicLMS Core, a headless LMS designed to be versatile and simple. App is multi tenant intended to be SAAS.

## Code Organization

### Architecture Patterns
- Use CQRS with MediatR
- Push logic to domain models rather than services
- Use a service layer extensively

### CQRS-Specific
- Commands should return only IDs, not full objects
- Queries should bypass domain logic entirely
- Event sourcing for critical student progress tracking

### Project Structure
- Group related functionality together by feature

### C# Standards
- Support a Fluent coding style where possible by returning this instead of void
- Mutate objects through predicates
- Public methods on the service layer, and domain modals, should include full XML documentation

### ASP.NET Core Standards
- Use as much config as possible, provided via IOptions pattern
- Use Automapper to map between Entities, Domain Models, and DTOs
- Use IHostedService for background tasks
- All API methods should have full OpenAPI documentation

## API Design
- The web API should be RESTful
- Use DTOs for request and response data
- Document API controllers and methods with OpenAPI
- API methods should catch exceptions
- Exceptions should not be used to control code flow
- Caught exceptions should be logged to ILogger
- API should be versioned using ApiVersionAttribute
- API methods will be put in to the feature bucket: Solution (management of tenants and other super-admin level calls), Administration (for a tenant to manage their setup), Learning (setting up courses), Learners (setting up users), and Engagement (actions by learners)
- Use ErrorResponseDto for all error responses to ensure consistency

## Identifiers
- Use hybrid ID approach: `InternalId` (int) for database primary key, `Id` (Guid) for public API exposure
- `InternalId` serves as the actual database primary key for performance and foreign key relationships
- `Id` (Guid) is the only identifier exposed in public APIs → prevents enumeration, supports distributed systems
- Never expose `InternalId` in DTOs, API responses, or public interfaces
- Generate Guid on entity creation, ensure it's indexed for query performance
- Use slugs/aliases only as secondary keys (for human-readable lookups or SEO) — always back them with a primary immutable ID
- Expose a canonical ID in endpoints (/entities/{id}), and provide alternate lookup routes when needed (/entities/by-slug/{slug})
- Prefer sequential GUIDs (UUIDv7/ULID) over random GUIDs to reduce index fragmentation when possible

## Domain-Specific Standards
- Soft deletes by default for all student/course data
- Immutable audit logs for all grade/enrollment changes
- Audit Fields should be set in the Infrastructure project, and readonly in the models
- All major entities should have metadata (IDictionary<string,string>)
- Metadata should not be returned in listing actions

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
- Authentication for all controllers should be provided through Auth0

## Validation
- Validate at the boundaries (controllers, message handlers) for request shape & permissions.
- Enforce business rules/invariants in the domain/service layer (so all call paths are covered).
- Repositories shouldn’t validate; they persist aggregates and rely on DB constraints as a safety net.
- Structure validation so rules live in one place and are reused (validators, value objects, entity methods).
