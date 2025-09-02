using AtomicLmsCore.Application.Tenants.Commands;
using AtomicLmsCore.Application.Tenants.Validators;
using FluentAssertions;

namespace AtomicLmsCore.Application.Tests.Tenants.Validators;

public class UpdateTenantCommandValidatorTests
{
    private readonly UpdateTenantCommandValidator _validator;

    public UpdateTenantCommandValidatorTests()
    {
        _validator = new UpdateTenantCommandValidator();
    }

    public class ValidateTests : UpdateTenantCommandValidatorTests
    {
        [Fact]
        public void Validate_WhenValidCommand_PassesValidation()
        {
            // Arrange
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                "test-tenant",
                true,
                new Dictionary<string, string> { { "key", "value" } });

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenIdEmpty_FailsValidation()
        {
            // Arrange
            var command = new UpdateTenantCommand(
                Guid.Empty,
                "Test Tenant",
                "test-tenant",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Tenant ID is required.");
        }

        [Fact]
        public void Validate_WhenNameEmpty_FailsValidation()
        {
            // Arrange
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "",
                "test-tenant",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Tenant name is required.");
        }

        [Fact]
        public void Validate_WhenNameTooLong_FailsValidation()
        {
            // Arrange
            var longName = new string('A', 256); // 256 chars
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                longName,
                "test-tenant",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Tenant name must not exceed 255 characters.");
        }

        [Fact]
        public void Validate_WhenSlugEmpty_FailsValidation()
        {
            // Arrange
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                "",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Tenant slug is required.");
        }

        [Fact]
        public void Validate_WhenSlugTooLong_FailsValidation()
        {
            // Arrange
            var longSlug = new string('a', 101); // 101 chars
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                longSlug,
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Tenant slug must not exceed 100 characters.");
        }

        [Theory]
        [InlineData("Test-Slug")]  // Uppercase
        [InlineData("test_slug")]  // Underscore
        [InlineData("test slug")]  // Space
        [InlineData("test@slug")]  // Special characters
        [InlineData("testSlug")]   // Mixed case
        public void Validate_WhenSlugInvalidFormat_FailsValidation(string invalidSlug)
        {
            // Arrange
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                invalidSlug,
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Tenant slug must contain only lowercase letters, numbers, and hyphens.");
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test-tenant")]
        [InlineData("tenant-123")]
        [InlineData("a")]
        [InlineData("123")]
        [InlineData("test-tenant-with-many-hyphens")]
        public void Validate_WhenSlugValidFormat_PassesValidation(string validSlug)
        {
            // Arrange
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                validSlug,
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_WhenIsActiveVariousValues_PassesValidation(bool isActive)
        {
            // Arrange
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                "test-tenant",
                isActive,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenMetadataNull_PassesValidation()
        {
            // Arrange
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                "test-tenant",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenMetadataEmpty_PassesValidation()
        {
            // Arrange
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                "test-tenant",
                true,
                new Dictionary<string, string>());

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenMetadataExceedsMaxItems_FailsValidation()
        {
            // Arrange
            var metadata = new Dictionary<string, string>();
            for (int i = 0; i < 51; i++)
            {
                metadata[$"key{i}"] = $"value{i}";
            }

            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                "test-tenant",
                true,
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Metadata cannot contain more than 50 items.");
        }

        [Fact]
        public void Validate_WhenMetadataKeyTooLong_FailsValidation()
        {
            // Arrange
            var longKey = new string('k', 101); // 101 chars
            var metadata = new Dictionary<string, string> { { longKey, "value" } };
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                "test-tenant",
                true,
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Metadata keys must not exceed 100 characters.");
        }

        [Fact]
        public void Validate_WhenMetadataValueTooLong_FailsValidation()
        {
            // Arrange
            var longValue = new string('v', 1001); // 1001 chars
            var metadata = new Dictionary<string, string> { { "key", longValue } };
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                "test-tenant",
                true,
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Metadata values must not exceed 1000 characters.");
        }

        [Fact]
        public void Validate_WhenMetadataWithinLimits_PassesValidation()
        {
            // Arrange
            var metadata = new Dictionary<string, string>();
            for (int i = 0; i < 50; i++)
            {
                var key = $"key{i}";
                var value = new string('v', 100); // Well within the 1000 char limit
                metadata[key] = value;
            }

            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                "test-tenant",
                true,
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenMultipleFieldsInvalid_ReturnsMultipleErrors()
        {
            // Arrange
            var command = new UpdateTenantCommand(
                Guid.Empty,
                "",
                "",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(3);
            result.Errors.Should().Contain(e => e.ErrorMessage == "Tenant ID is required.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Tenant name is required.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Tenant slug is required.");
        }

        [Fact]
        public void Validate_WhenMultipleMetadataErrorsExist_ReturnsMultipleErrors()
        {
            // Arrange
            var longKey = new string('k', 101);
            var longValue = new string('v', 1001);
            var metadata = new Dictionary<string, string> { { longKey, longValue } };
            var command = new UpdateTenantCommand(
                Guid.NewGuid(),
                "Test Tenant",
                "test-tenant",
                true,
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain(e => e.ErrorMessage == "Metadata keys must not exceed 100 characters.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Metadata values must not exceed 1000 characters.");
        }
    }
}