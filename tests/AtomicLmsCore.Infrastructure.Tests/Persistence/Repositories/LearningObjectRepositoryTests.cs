using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Infrastructure.Persistence;
using AtomicLmsCore.Infrastructure.Persistence.Repositories;
using AtomicLmsCore.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AtomicLmsCore.Infrastructure.Tests.Persistence.Repositories;

public class LearningObjectRepositoryTests : IDisposable
{
    private readonly TenantDbContext _context;
    private readonly LearningObjectRepository _repository;

    public LearningObjectRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TenantDbContext(options, new UlidIdGenerator());
        _repository = new LearningObjectRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ValidLearningObject_ReturnsLearningObject()
    {
        // Arrange
        var learningObject = new LearningObject
        {
            Id = Guid.NewGuid(),
            Name = "Test Learning Object",
            Metadata = new Dictionary<string, string> { { "key", "value" } }
        };

        // Act
        var result = await _repository.AddAsync(learningObject);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Learning Object");
        result.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingLearningObject_ReturnsLearningObject()
    {
        // Arrange
        var learningObject = new LearningObject
        {
            Id = Guid.NewGuid(),
            Name = "Test Learning Object",
            Metadata = new Dictionary<string, string>()
        };
        await _repository.AddAsync(learningObject);

        // Act
        var result = await _repository.GetByIdAsync(learningObject.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Learning Object");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentLearningObject_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultipleLearningObjects_ReturnsAllOrderedByName()
    {
        // Arrange
        var learningObject1 = new LearningObject
        {
            Id = Guid.NewGuid(),
            Name = "Beta Learning Object",
            Metadata = new Dictionary<string, string>()
        };
        var learningObject2 = new LearningObject
        {
            Id = Guid.NewGuid(),
            Name = "Alpha Learning Object",
            Metadata = new Dictionary<string, string>()
        };

        await _repository.AddAsync(learningObject1);
        await _repository.AddAsync(learningObject2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Alpha Learning Object");
        result[1].Name.Should().Be("Beta Learning Object");
    }

    [Fact]
    public async Task UpdateAsync_ExistingLearningObject_UpdatesSuccessfully()
    {
        // Arrange
        var learningObject = new LearningObject
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Metadata = new Dictionary<string, string>()
        };
        await _repository.AddAsync(learningObject);

        learningObject.Name = "Updated Name";
        learningObject.Metadata["newKey"] = "newValue";

        // Act
        var result = await _repository.UpdateAsync(learningObject);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.Metadata.Should().ContainKey("newKey");

        var retrieved = await _repository.GetByIdAsync(learningObject.Id);
        retrieved!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_ExistingLearningObject_SoftDeletesSuccessfully()
    {
        // Arrange
        var learningObject = new LearningObject
        {
            Id = Guid.NewGuid(),
            Name = "To Be Deleted",
            Metadata = new Dictionary<string, string>()
        };
        await _repository.AddAsync(learningObject);

        // Act
        var result = await _repository.DeleteAsync(learningObject.Id);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete - should not be returned by normal queries
        var retrieved = await _repository.GetByIdAsync(learningObject.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentLearningObject_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ExistingLearningObject_ReturnsTrue()
    {
        // Arrange
        var learningObject = new LearningObject
        {
            Id = Guid.NewGuid(),
            Name = "Existing Object",
            Metadata = new Dictionary<string, string>()
        };
        await _repository.AddAsync(learningObject);

        // Act
        var result = await _repository.ExistsAsync(learningObject.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistentLearningObject_ReturnsFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.ExistsAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
