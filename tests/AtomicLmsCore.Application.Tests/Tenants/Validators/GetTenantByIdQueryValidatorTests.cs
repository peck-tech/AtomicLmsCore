using AtomicLmsCore.Application.Tenants.Queries;
using AtomicLmsCore.Application.Tenants.Validators;
using FluentAssertions;

namespace AtomicLmsCore.Application.Tests.Tenants.Validators;

public class GetTenantByIdQueryValidatorTests
{
    private readonly GetTenantByIdQueryValidator _validator;

    public GetTenantByIdQueryValidatorTests()
    {
        _validator = new GetTenantByIdQueryValidator();
    }

    public class ValidateTests : GetTenantByIdQueryValidatorTests
    {
        [Fact]
        public void Validate_WhenValidQuery_PassesValidation()
        {
            // Arrange
            var query = new GetTenantByIdQuery(Guid.NewGuid());

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenIdEmpty_FailsValidation()
        {
            // Arrange
            var query = new GetTenantByIdQuery(Guid.Empty);

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Tenant ID is required.");
        }

        [Fact]
        public void Validate_WhenValidGuid_PassesValidation()
        {
            // Arrange
            var validId = new Guid("12345678-1234-1234-1234-123456789012");
            var query = new GetTenantByIdQuery(validId);

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenRandomGuid_PassesValidation()
        {
            // Arrange
            var query = new GetTenantByIdQuery(Guid.NewGuid());

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenMultipleQueries_EachValidatedIndependently()
        {
            // Arrange
            var validQuery = new GetTenantByIdQuery(Guid.NewGuid());
            var invalidQuery = new GetTenantByIdQuery(Guid.Empty);

            // Act
            var validResult = _validator.Validate(validQuery);
            var invalidResult = _validator.Validate(invalidQuery);

            // Assert
            validResult.IsValid.Should().BeTrue();
            invalidResult.IsValid.Should().BeFalse();
            invalidResult.Errors.Should().ContainSingle(e => e.ErrorMessage == "Tenant ID is required.");
        }
    }
}