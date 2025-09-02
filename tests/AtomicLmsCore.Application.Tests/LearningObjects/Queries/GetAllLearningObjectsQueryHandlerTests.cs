using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.LearningObjects.Queries;
using AtomicLmsCore.Domain.Entities;
using FluentAssertions;
using Moq;

namespace AtomicLmsCore.Application.Tests.LearningObjects.Queries;

public class GetAllLearningObjectsQueryHandlerTests
{
    private readonly GetAllLearningObjectsQueryHandler _handler;
    private readonly Mock<ILearningObjectRepository> _repositoryMock;

    public GetAllLearningObjectsQueryHandlerTests()
    {
        _repositoryMock = new Mock<ILearningObjectRepository>();
        _handler = new GetAllLearningObjectsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenLearningObjectsExist_ReturnsSuccess()
    {
        // Arrange
        var learningObjects = new List<LearningObject>
        {
            new() { Id = Guid.NewGuid(), Name = "Learning Object 1", Metadata = new Dictionary<string, string>() },
            new() { Id = Guid.NewGuid(), Name = "Learning Object 2", Metadata = new Dictionary<string, string>() }
        };

        var query = new GetAllLearningObjectsQuery();
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(learningObjects);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(learningObjects);
        _repositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoLearningObjectsExist_ReturnsEmptyList()
    {
        // Arrange
        var learningObjects = new List<LearningObject>();
        var query = new GetAllLearningObjectsQuery();
        
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(learningObjects);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
        _repositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var query = new GetAllLearningObjectsQuery();
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Failed to retrieve learning objects: Database error");
        _repositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCancellationRequested_PassesCancellationToken()
    {
        // Arrange
        var learningObjects = new List<LearningObject>();
        var query = new GetAllLearningObjectsQuery();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _repositoryMock.Setup(x => x.GetAllAsync(cancellationToken))
            .ReturnsAsync(learningObjects);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLearningObjectsHaveMetadata_ReturnsWithMetadata()
    {
        // Arrange
        var learningObjects = new List<LearningObject>
        {
            new() 
            { 
                Id = Guid.NewGuid(), 
                Name = "Learning Object with Metadata", 
                Metadata = new Dictionary<string, string> 
                { 
                    { "author", "John Doe" }, 
                    { "version", "1.0" } 
                } 
            }
        };

        var query = new GetAllLearningObjectsQuery();
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(learningObjects);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Metadata.Should().ContainKey("author");
        result.Value.First().Metadata.Should().ContainKey("version");
        result.Value.First().Metadata["author"].Should().Be("John Doe");
        result.Value.First().Metadata["version"].Should().Be("1.0");
    }

    [Fact]
    public async Task Handle_MultipleCalls_CallsRepositoryEachTime()
    {
        // Arrange
        var learningObjects = new List<LearningObject>();
        var query = new GetAllLearningObjectsQuery();
        
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(learningObjects);

        // Act
        await _handler.Handle(query, CancellationToken.None);
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}