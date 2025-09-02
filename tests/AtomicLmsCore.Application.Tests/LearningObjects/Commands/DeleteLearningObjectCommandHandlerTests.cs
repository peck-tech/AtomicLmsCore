using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.LearningObjects.Commands;
using FluentAssertions;
using Moq;

namespace AtomicLmsCore.Application.Tests.LearningObjects.Commands;

public class DeleteLearningObjectCommandHandlerTests
{
    private readonly Mock<ILearningObjectRepository> _mockRepository;
    private readonly DeleteLearningObjectCommandHandler _handler;

    public DeleteLearningObjectCommandHandlerTests()
    {
        _mockRepository = new Mock<ILearningObjectRepository>();
        _handler = new DeleteLearningObjectCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteLearningObjectCommand(id);

        _mockRepository.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LearningObjectNotFound_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteLearningObjectCommand(id);

        _mockRepository.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain($"Learning object with ID {id} not found.");
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteLearningObjectCommand(id);

        _mockRepository.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Failed to delete learning object: Database error");
    }

    [Fact]
    public async Task Handle_CancellationRequested_PassesCancellationToken()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteLearningObjectCommand(id);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockRepository.Setup(x => x.DeleteAsync(id, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(id, cancellationToken), Times.Once);
    }
}
