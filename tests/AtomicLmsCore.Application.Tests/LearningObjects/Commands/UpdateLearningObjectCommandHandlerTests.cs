using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.LearningObjects.Commands;
using AtomicLmsCore.Domain.Entities;
using FluentAssertions;
using Moq;

namespace AtomicLmsCore.Application.Tests.LearningObjects.Commands;

public class UpdateLearningObjectCommandHandlerTests
{
    private readonly Mock<ILearningObjectRepository> _mockRepository;
    private readonly UpdateLearningObjectCommandHandler _handler;

    public UpdateLearningObjectCommandHandlerTests()
    {
        _mockRepository = new Mock<ILearningObjectRepository>();
        _handler = new UpdateLearningObjectCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var originalMetadata = new Dictionary<string, string> { { "old", "value" } };
        var updatedMetadata = new Dictionary<string, string> { { "new", "value" } };
        var existingLearningObject = new LearningObject { Id = id, Name = "Old Name", Metadata = originalMetadata };
        var command = new UpdateLearningObjectCommand(id, "Updated Name", updatedMetadata);

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLearningObject);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<LearningObject>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LearningObject lo, CancellationToken _) => lo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(
            It.Is<LearningObject>(lo =>
                lo.Id == id &&
                lo.Name == "Updated Name" &&
                lo.Metadata.SequenceEqual(updatedMetadata)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullMetadata_SetsEmptyMetadata()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingLearningObject = new LearningObject
        {
            Id = id,
            Name = "Old Name",
            Metadata = new Dictionary<string, string> { { "old", "value" } }
        };
        var command = new UpdateLearningObjectCommand(id, "Updated Name", null);

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLearningObject);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<LearningObject>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LearningObject lo, CancellationToken _) => lo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.UpdateAsync(
            It.Is<LearningObject>(lo =>
                lo.Id == id &&
                lo.Name == "Updated Name" &&
                lo.Metadata.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LearningObjectNotFound_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateLearningObjectCommand(id, "Updated Name");

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LearningObject?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain($"Learning object with ID {id} not found.");
        _mockRepository.Verify(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<LearningObject>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GetByIdThrowsException_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateLearningObjectCommand(id, "Updated Name");

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Failed to update learning object: Database error");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<LearningObject>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdateThrowsException_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingLearningObject = new LearningObject { Id = id, Name = "Old Name" };
        var command = new UpdateLearningObjectCommand(id, "Updated Name");

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLearningObject);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<LearningObject>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Failed to update learning object: Update failed");
    }

    [Fact]
    public async Task Handle_CancellationRequested_PassesCancellationToken()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingLearningObject = new LearningObject { Id = id, Name = "Old Name" };
        var command = new UpdateLearningObjectCommand(id, "Updated Name");
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockRepository.Setup(x => x.GetByIdAsync(id, cancellationToken))
            .ReturnsAsync(existingLearningObject);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<LearningObject>(), cancellationToken))
            .ReturnsAsync((LearningObject lo, CancellationToken _) => lo);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.GetByIdAsync(id, cancellationToken), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<LearningObject>(), cancellationToken), Times.Once);
    }
}
