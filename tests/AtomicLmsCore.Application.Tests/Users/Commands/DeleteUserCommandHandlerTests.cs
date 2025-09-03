using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Users.Commands;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Application.Tests.Users.Commands;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<ILogger<DeleteUserCommandHandler>> _mockLogger;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<DeleteUserCommandHandler>>();
        _handler = new DeleteUserCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteUserCommand(id);

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok((Domain.Entities.User?)null));

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok((Domain.Entities.User?)null));

        _mockRepository.Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(id), Times.Once);
        VerifyLoggerWasCalled(LogLevel.Information, "User deleted successfully with ID");
    }

    [Fact]
    public async Task Handle_RepositoryDeleteFails_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteUserCommand(id);
        const string RepositoryError = "User not found";

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok((Domain.Entities.User?)null));

        _mockRepository.Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(Result.Fail(RepositoryError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be(RepositoryError);
        _mockRepository.Verify(x => x.DeleteAsync(id), Times.Once);
        VerifyLoggerWasNotCalled(LogLevel.Information);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteUserCommand(id);

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok((Domain.Entities.User?)null));

        _mockRepository.Setup(x => x.DeleteAsync(id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Failed to delete user");
        _mockRepository.Verify(x => x.DeleteAsync(id), Times.Once);
        VerifyLoggerWasCalled(LogLevel.Error, "Error deleting user");
    }

    [Fact]
    public async Task Handle_RepositoryReturnsMultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteUserCommand(id);
        var errors = new List<IError>
        {
            new Error("Error 1"),
            new Error("Error 2")
        };

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok<Domain.Entities.User?>(null));

        _mockRepository.Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(Result.Fail(errors));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Message == "Error 1");
        result.Errors.Should().Contain(e => e.Message == "Error 2");
    }

    [Fact]
    public async Task Handle_CancellationRequested_PassesCancellationToken()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteUserCommand(id);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(Result.Ok((Domain.Entities.User?)null));

        _mockRepository.Setup(x => x.DeleteAsync(id))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(id), Times.Once);
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

    private void VerifyLoggerWasNotCalled(LogLevel level)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
