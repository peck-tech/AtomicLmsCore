using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Users.Commands;
using AtomicLmsCore.Domain.Entities;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Application.Tests.Users.Commands;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IIdentityManagementService> _mockIdentityManagementService;
    private readonly Mock<ILogger<UpdateUserCommandHandler>> _mockLogger;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockIdentityManagementService = new Mock<IIdentityManagementService>();
        _mockLogger = new Mock<ILogger<UpdateUserCommandHandler>>();
        _handler = new UpdateUserCommandHandler(_mockRepository.Object, _mockIdentityManagementService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingUser = new User
        {
            Id = id,
            Email = "old@example.com",
            FirstName = "Old First",
            LastName = "Old Last",
            DisplayName = "Old Display",
            IsActive = true,
            Metadata = new Dictionary<string, string>()
        };
        var command = new UpdateUserCommand(
            id,
            "new@example.com",
            "New First",
            "New Last",
            "New Display",
            false,
            new Dictionary<string, string> { { "key", "value" } });

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok<User?>(existingUser));
        _mockRepository.Setup(x => x.EmailExistsAsync("new@example.com", id))
            .ReturnsAsync(Result.Ok(false));
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(Result.Ok());
        _mockIdentityManagementService.Setup(x => x.UpdateUserMetadataAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.GetByIdAsync(id), Times.Once);
        _mockRepository.Verify(x => x.EmailExistsAsync("new@example.com", id), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(
            It.Is<User>(u =>
                u.Id == id &&
                u.Email == "new@example.com" &&
                u.FirstName == "New First" &&
                u.LastName == "New Last" &&
                u.DisplayName == "New Display" &&
                u.IsActive == false &&
                u.Metadata.ContainsKey("key"))),
            Times.Once);
        VerifyLoggerWasCalled(LogLevel.Information, "User updated successfully with ID");
    }

    [Fact]
    public async Task Handle_SameEmail_SkipsEmailValidation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingUser = new User
        {
            Id = id,
            Email = "same@example.com",
            FirstName = "Old First",
            LastName = "Old Last",
            DisplayName = "Old Display",
            IsActive = true
        };
        var command = new UpdateUserCommand(
            id,
            "same@example.com",
            "New First",
            "New Last",
            "New Display",
            false,
            null);

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok<User?>(existingUser));
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>()), Times.Never);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullMetadata_DoesNotUpdateMetadata()
    {
        // Arrange
        var id = Guid.NewGuid();
        var originalMetadata = new Dictionary<string, string> { { "original", "value" } };
        var existingUser = new User
        {
            Id = id,
            Email = "test@example.com",
            FirstName = "First",
            LastName = "Last",
            DisplayName = "Display",
            IsActive = true,
            Metadata = originalMetadata
        };
        var command = new UpdateUserCommand(id, "test@example.com", "New First", "New Last", "New Display", true, null);

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok<User?>(existingUser));
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.UpdateAsync(
            It.Is<User>(u => u.Metadata == originalMetadata)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateUserCommand(id, "test@example.com", "First", "Last", "Display", true, null);

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok<User?>(null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("User not found");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GetByIdFails_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateUserCommand(id, "test@example.com", "First", "Last", "Display", true, null);

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Fail("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("User not found");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingUser = new User
        {
            Id = id,
            Email = "old@example.com",
            FirstName = "First",
            LastName = "Last",
            DisplayName = "Display",
            IsActive = true
        };
        var command = new UpdateUserCommand(id, "existing@example.com", "First", "Last", "Display", true, null);

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok<User?>(existingUser));
        _mockRepository.Setup(x => x.EmailExistsAsync("existing@example.com", id))
            .ReturnsAsync(Result.Ok(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("A user with this email already exists in the tenant");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailExistsCheckFails_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingUser = new User
        {
            Id = id,
            Email = "old@example.com",
            FirstName = "First",
            LastName = "Last",
            DisplayName = "Display",
            IsActive = true
        };
        var command = new UpdateUserCommand(id, "new@example.com", "First", "Last", "Display", true, null);

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok<User?>(existingUser));
        _mockRepository.Setup(x => x.EmailExistsAsync("new@example.com", id))
            .ReturnsAsync(Result.Fail("Email check failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Email check failed");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateFails_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingUser = new User
        {
            Id = id,
            Email = "test@example.com",
            FirstName = "First",
            LastName = "Last",
            DisplayName = "Display",
            IsActive = true
        };
        var command = new UpdateUserCommand(id, "test@example.com", "New First", "New Last", "New Display", false, null);

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok<User?>(existingUser));
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(Result.Fail("Update failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Update failed");
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateUserCommand(id, "test@example.com", "First", "Last", "Display", true, null);

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Failed to update user");
        VerifyLoggerWasCalled(LogLevel.Error, "Error updating user");
    }

    private void VerifyLoggerWasCalled(LogLevel level, string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
