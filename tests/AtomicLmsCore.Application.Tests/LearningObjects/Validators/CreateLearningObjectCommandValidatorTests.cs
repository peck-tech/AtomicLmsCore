using AtomicLmsCore.Application.LearningObjects.Commands;
using AtomicLmsCore.Application.LearningObjects.Validators;
using FluentValidation.TestHelper;
// ReSharper disable RedundantArgumentDefaultValue

namespace AtomicLmsCore.Application.Tests.LearningObjects.Validators;

public class CreateLearningObjectCommandValidatorTests
{
    private readonly CreateLearningObjectCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateLearningObjectCommand("Valid Name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_EmptyOrNullName_ShouldHaveValidationError(string? name)
    {
        // Arrange
        var command = new CreateLearningObjectCommand(name!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Learning object name is required.");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('a', 501);
        var command = new CreateLearningObjectCommand(longName);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Learning object name cannot exceed 500 characters.");
    }

    [Fact]
    public void Validate_MetadataWithEmptyKey_ShouldHaveValidationError()
    {
        // Arrange
        var metadata = new Dictionary<string, string> { { "", "value" } };
        var command = new CreateLearningObjectCommand("Valid Name", metadata);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata)
            .WithErrorMessage("Metadata keys cannot be empty or whitespace.");
    }

    [Fact]
    public void Validate_MetadataWithNullValue_ShouldHaveValidationError()
    {
        // Arrange
        var metadata = new Dictionary<string, string> { { "key", null! } };
        var command = new CreateLearningObjectCommand("Valid Name", metadata);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Metadata)
            .WithErrorMessage("Metadata values cannot be null.");
    }

    [Fact]
    public void Validate_ValidMetadata_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var metadata = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };
        var command = new CreateLearningObjectCommand("Valid Name", metadata);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NullMetadata_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateLearningObjectCommand("Valid Name", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
