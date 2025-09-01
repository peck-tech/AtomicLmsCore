using AtomicLmsCore.Application.Users.Commands;
using AtomicLmsCore.Application.Users.Validators;
using FluentAssertions;

namespace AtomicLmsCore.Application.Tests.Users.Validators;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator;

    public CreateUserCommandValidatorTests()
    {
        _validator = new CreateUserCommandValidator();
    }

    public class ValidateTests : CreateUserCommandValidatorTests
    {
        [Fact]
        public void Validate_WhenValidCommand_PassesValidation()
        {
            // Arrange
            var command = new CreateUserCommand(
                "external|test123",
                "test@example.com",
                "John",
                "Doe",
                "John Doe",
                true,
                new Dictionary<string, string>());

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WhenExternalUserIdEmpty_FailsValidation()
        {
            // Arrange
            var command = new CreateUserCommand(
                "",
                "test@example.com",
                "John",
                "Doe",
                "John Doe",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "External User ID is required");
        }

        [Fact]
        public void Validate_WhenEmailEmpty_FailsValidation()
        {
            // Arrange
            var command = new CreateUserCommand(
                "external|test123",
                "",
                "John",
                "Doe",
                "John Doe",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Email is required");
        }

        [Fact]
        public void Validate_WhenEmailInvalid_FailsValidation()
        {
            // Arrange
            var command = new CreateUserCommand(
                "external|test123",
                "invalid-email",
                "John",
                "Doe",
                "John Doe",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Email must be a valid email address");
        }

        [Fact]
        public void Validate_WhenFirstNameEmpty_FailsValidation()
        {
            // Arrange
            var command = new CreateUserCommand(
                "external|test123",
                "test@example.com",
                "",
                "Doe",
                "John Doe",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "First name is required");
        }

    }
}
