# Unit Test Policy

## Coverage Requirements
- Minimum 80% code coverage for new features
- Critical business logic must have 100% coverage
- All public service methods require tests
- All domain model methods require tests

## Test Structure
- Follow AAA pattern: Arrange, Act, Assert
- One assertion per test method
- Test method names: `MethodName_Scenario_ExpectedResult()`
- Group related tests in nested classes using `[TestClass]` for scenarios

## What to Test
### Required
- All public service layer methods
- Domain model business logic
- Validation rules and FluentValidation validators
- CQRS command and query handlers
- Critical algorithms (grade calculations, enrollment state)
- Error handling paths

### Optional
- Private methods (test through public interface)
- Simple DTOs and POCOs
- Auto-generated code
- Framework configuration

## Testing Standards
### Mocking
- Use Moq for dependencies
- Mock at service boundaries only
- Don't mock domain models
- Use builders for complex test data

### Database Testing
- Use in-memory database for integration tests
- Test repositories with actual EF Core context
- Verify database constraints and relationships
- Test migrations separately

### Async Testing
- Use `async Task` for async test methods
- Always await async operations
- Test cancellation tokens where applicable

## Test Organization
- Mirror production code structure in test projects
- Suffix test projects with `.Tests`
- Place unit tests in `Tests/Unit` folder
- Place integration tests in `Tests/Integration` folder

## Assertions
- Use FluentAssertions for readable assertions
- Verify FluentResult success/failure states
- Check exception types and messages
- Validate returned IDs for commands

## Test Data
- Use object mothers or builders for complex objects
- Keep test data minimal and focused
- Use constants for repeated test values
- Clean up test data in teardown when needed

## Performance
- Unit tests should complete in < 100ms
- Mark slow tests with `[TestCategory("Slow")]`
- Run fast tests on every build
- Run slow tests in CI pipeline only

## Integration Testing
### API Integration Tests
- Use WebApplicationFactory for full HTTP request/response testing
- Test all API endpoints for authentication and authorization
- Verify HTTP status codes, response bodies, and headers
- Test error scenarios and edge cases
- Use in-memory databases for test isolation

### Integration Test Requirements
- All API controllers must have comprehensive integration tests
- Test happy path and error scenarios for each endpoint
- Verify tenant isolation and multi-tenant functionality
- Test authentication flows and role-based access
- Include correlation ID validation in responses

### Test Data Management
- Use test-specific databases (in-memory or test containers)
- Seed minimal test data for each test scenario
- Clean up test data between test runs
- Mock external dependencies and services

### API Changes Policy
- **MANDATORY**: When adding new API endpoints, create corresponding integration tests
- **MANDATORY**: When modifying existing endpoints, update integration tests accordingly
- **MANDATORY**: When changing authentication/authorization, verify with integration tests
- Integration tests must cover all HTTP methods (GET, POST, PUT, DELETE)
- Test both successful responses and error conditions

## Continuous Integration
- Tests must pass before merging
- Failed tests block deployment
- Monitor test execution time trends
- Maintain test reliability (no flaky tests)
- Integration tests run in CI pipeline alongside unit tests