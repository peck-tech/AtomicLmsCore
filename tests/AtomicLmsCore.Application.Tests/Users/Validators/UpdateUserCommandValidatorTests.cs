using AtomicLmsCore.Application.Users.Commands;
using AtomicLmsCore.Application.Users.Validators;
using FluentAssertions;

namespace AtomicLmsCore.Application.Tests.Users.Validators;

public class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _validator;

    public UpdateUserCommandValidatorTests()
    {
        _validator = new UpdateUserCommandValidator();
    }

    public class ValidateTests : UpdateUserCommandValidatorTests
    {
        [Fact]
        public void Validate_WhenValidCommand_PassesValidation()
        {
            // Arrange
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
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
        public void Validate_WhenIdEmpty_FailsValidation()
        {
            // Arrange
            var command = new UpdateUserCommand(
                Guid.Empty,
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
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "User ID is required");
        }

        [Fact]
        public void Validate_WhenEmailEmpty_FailsValidation()
        {
            // Arrange
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
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
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
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
        public void Validate_WhenEmailTooLong_FailsValidation()
        {
            // Arrange
            var longEmail = new string('a', 250) + "@example.com"; // 265 chars total
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
                longEmail,
                "John",
                "Doe",
                "John Doe",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Email must not exceed 255 characters");
        }

        [Fact]
        public void Validate_WhenFirstNameEmpty_FailsValidation()
        {
            // Arrange
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
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

        [Fact]
        public void Validate_WhenFirstNameTooLong_FailsValidation()
        {
            // Arrange
            var longFirstName = new string('A', 101); // 101 chars
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
                "test@example.com",
                longFirstName,
                "Doe",
                "John Doe",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "First name must not exceed 100 characters");
        }

        [Fact]
        public void Validate_WhenLastNameEmpty_FailsValidation()
        {
            // Arrange
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
                "test@example.com",
                "John",
                "",
                "John Doe",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Last name is required");
        }

        [Fact]
        public void Validate_WhenLastNameTooLong_FailsValidation()
        {
            // Arrange
            var longLastName = new string('B', 101); // 101 chars
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
                "test@example.com",
                "John",
                longLastName,
                "John Doe",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Last name must not exceed 100 characters");
        }

        [Fact]
        public void Validate_WhenDisplayNameEmpty_FailsValidation()
        {
            // Arrange
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
                "test@example.com",
                "John",
                "Doe",
                "",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Display name is required");
        }

        [Fact]
        public void Validate_WhenDisplayNameTooLong_FailsValidation()
        {
            // Arrange
            var longDisplayName = new string('C', 201); // 201 chars
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
                "test@example.com",
                "John",
                "Doe",
                longDisplayName,
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.ErrorMessage == "Display name must not exceed 200 characters");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Validate_WhenIsActiveVariousValues_PassesValidation(bool isActive)
        {
            // Arrange
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
                "test@example.com",
                "John",
                "Doe",
                "John Doe",
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
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
                "test@example.com",
                "John",
                "Doe",
                "John Doe",
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
            var command = new UpdateUserCommand(
                Guid.NewGuid(),
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
        public void Validate_WhenMultipleFieldsInvalid_ReturnsMultipleErrors()
        {
            // Arrange
            var command = new UpdateUserCommand(
                Guid.Empty,
                "",
                "",
                "",
                "",
                true,
                null);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(6); // Empty email triggers both required and valid email validation
            result.Errors.Should().Contain(e => e.ErrorMessage == "User ID is required");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email is required");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Email must be a valid email address");
            result.Errors.Should().Contain(e => e.ErrorMessage == "First name is required");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Last name is required");
            result.Errors.Should().Contain(e => e.ErrorMessage == "Display name is required");
        }
    }
}
