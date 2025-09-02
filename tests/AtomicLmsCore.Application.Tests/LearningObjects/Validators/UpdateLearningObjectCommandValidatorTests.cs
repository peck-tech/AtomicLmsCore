using AtomicLmsCore.Application.LearningObjects.Commands;
using AtomicLmsCore.Application.LearningObjects.Validators;
using FluentAssertions;

namespace AtomicLmsCore.Application.Tests.LearningObjects.Validators;

public class UpdateLearningObjectCommandValidatorTests
{
    private readonly UpdateLearningObjectCommandValidator _validator;

    public UpdateLearningObjectCommandValidatorTests()
    {
        _validator = new UpdateLearningObjectCommandValidator();
    }

    public class ValidateTests : UpdateLearningObjectCommandValidatorTests
    {
        [Fact]
        public void Validate_WhenValidCommand_PassesValidation()
        {
            // Arrange
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "Test Learning Object",
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
            var command = new UpdateLearningObjectCommand(
                Guid.Empty,
                "Test Learning Object",
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Learning object ID is required.");
        }

        [Fact]
        public void Validate_WhenNameEmpty_FailsValidation()
        {
            // Arrange
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "",
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Learning object name is required.");
        }

        [Fact]
        public void Validate_WhenNameWhitespace_FailsValidation()
        {
            // Arrange
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "   ",
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Learning object name is required.");
        }

        [Fact]
        public void Validate_WhenNameTooLong_FailsValidation()
        {
            // Arrange
            var longName = new string('A', 501); // 501 chars
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                longName,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Learning object name cannot exceed 500 characters.");
        }

        [Fact]
        public void Validate_WhenNameAtMaxLength_PassesValidation()
        {
            // Arrange
            var maxLengthName = new string('A', 500); // 500 chars - at the limit
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                maxLengthName,
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
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "Test Learning Object",
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
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "Test Learning Object",
                new Dictionary<string, string>());

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenMetadataHasValidKeyValue_PassesValidation()
        {
            // Arrange
            var metadata = new Dictionary<string, string>
            {
                { "author", "John Doe" },
                { "version", "1.0" },
                { "tags", "education,learning" }
            };
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "Test Learning Object",
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Validate_WhenMetadataKeyEmptyOrWhitespace_FailsValidation(string invalidKey)
        {
            // Arrange
            var metadata = new Dictionary<string, string> { { invalidKey, "value" } };
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "Test Learning Object",
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Metadata keys cannot be empty or whitespace.");
        }

        [Fact]
        public void Validate_WhenMetadataValueNull_FailsValidation()
        {
            // Arrange
            var metadata = new Dictionary<string, string> { { "key", null! } };
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "Test Learning Object",
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Metadata values cannot be null.");
        }

        [Fact]
        public void Validate_WhenMetadataValueEmpty_PassesValidation()
        {
            // Arrange
            var metadata = new Dictionary<string, string> { { "key", "" } };
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "Test Learning Object",
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenMultipleMetadataKeysInvalid_ReturnsAllErrors()
        {
            // Arrange
            var metadata = new Dictionary<string, string>
            {
                { "", "value1" },
                { "   ", "value2" },
                { "validkey", null! }
            };
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "Test Learning Object",
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain(e => e.ErrorMessage == "Metadata keys cannot be empty or whitespace.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Metadata values cannot be null.");
        }

        [Fact]
        public void Validate_WhenMultipleFieldsInvalid_ReturnsMultipleErrors()
        {
            // Arrange
            var metadata = new Dictionary<string, string> { { "", null! } };
            var command = new UpdateLearningObjectCommand(
                Guid.Empty,
                "",
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(4);
            result.Errors.Should().Contain(e => e.ErrorMessage == "Learning object ID is required.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Learning object name is required.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Metadata keys cannot be empty or whitespace.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Metadata values cannot be null.");
        }

        [Fact]
        public void Validate_WhenMetadataHasSpecialCharactersInKey_PassesValidation()
        {
            // Arrange
            var metadata = new Dictionary<string, string>
            {
                { "key-with-dashes", "value1" },
                { "key_with_underscores", "value2" },
                { "key.with.dots", "value3" },
                { "key@with@symbols", "value4" }
            };
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "Test Learning Object",
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenMetadataHasLongValues_PassesValidation()
        {
            // Arrange
            var longValue = new string('V', 5000); // Very long value
            var metadata = new Dictionary<string, string> { { "longvalue", longValue } };
            var command = new UpdateLearningObjectCommand(
                Guid.NewGuid(),
                "Test Learning Object",
                metadata);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
