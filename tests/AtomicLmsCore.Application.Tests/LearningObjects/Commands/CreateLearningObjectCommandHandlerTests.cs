using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.LearningObjects.Commands;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using FluentAssertions;
using Moq;

namespace AtomicLmsCore.Application.Tests.LearningObjects.Commands;

public class CreateLearningObjectCommandHandlerTests
{
    private readonly Mock<ILearningObjectRepository> _mockRepository;
    private readonly Mock<IIdGenerator> _mockIdGenerator;
    private readonly CreateLearningObjectCommandHandler _handler;

    public CreateLearningObjectCommandHandlerTests()
    {
        _mockRepository = new Mock<ILearningObjectRepository>();
        _mockIdGenerator = new Mock<IIdGenerator>();
        _handler = new CreateLearningObjectCommandHandler(_mockRepository.Object, _mockIdGenerator.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithId()
    {
        // Arrange
        var generatedId = Guid.NewGuid();
        var command = new CreateLearningObjectCommand("Test Learning Object");

        _mockIdGenerator.Setup(x => x.NewId()).Returns(generatedId);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<LearningObject>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LearningObject lo, CancellationToken _) => lo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(generatedId);
    }

    [Fact]
    public async Task Handle_ValidCommandWithMetadata_CreatesLearningObjectWithMetadata()
    {
        // Arrange
        var generatedId = Guid.NewGuid();
        var metadata = new Dictionary<string, string> { { "key1", "value1" } };
        var command = new CreateLearningObjectCommand("Test Learning Object", metadata);

        _mockIdGenerator.Setup(x => x.NewId()).Returns(generatedId);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<LearningObject>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LearningObject lo, CancellationToken _) => lo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.AddAsync(
            It.Is<LearningObject>(lo =>
                lo.Name == "Test Learning Object" &&
                lo.Metadata.SequenceEqual(metadata)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var command = new CreateLearningObjectCommand("Test Learning Object");
        var generatedId = Guid.NewGuid();

        _mockIdGenerator.Setup(x => x.NewId()).Returns(generatedId);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<LearningObject>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Failed to create learning object");
    }

    [Fact]
    public async Task Handle_NullMetadata_CreatesEmptyMetadata()
    {
        // Arrange
        var generatedId = Guid.NewGuid();
        var command = new CreateLearningObjectCommand("Test Learning Object", null);

        _mockIdGenerator.Setup(x => x.NewId()).Returns(generatedId);
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<LearningObject>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LearningObject lo, CancellationToken _) => lo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.AddAsync(
            It.Is<LearningObject>(lo => lo.Metadata.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
