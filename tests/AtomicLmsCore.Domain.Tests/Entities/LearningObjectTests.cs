using AtomicLmsCore.Domain.Entities;
using FluentAssertions;

namespace AtomicLmsCore.Domain.Tests.Entities;

public class LearningObjectTests
{
    [Fact]
    public void Constructor_DefaultValues_SetsExpectedDefaults()
    {
        // Act
        var learningObject = new LearningObject();

        // Assert
        learningObject.Name.Should().Be(string.Empty);
        learningObject.Metadata.Should().NotBeNull();
        learningObject.Metadata.Should().BeEmpty();
        learningObject.Id.Should().Be(Guid.Empty);
        learningObject.InternalId.Should().Be(0);
        learningObject.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Name_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var learningObject = new LearningObject();
        var expectedName = "Test Learning Object";

        // Act
        learningObject.Name = expectedName;

        // Assert
        learningObject.Name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Short")]
    [InlineData("This is a very long learning object name that should still be handled correctly")]
    public void Name_SetVariousValues_UpdatesCorrectly(string name)
    {
        // Arrange
        var learningObject = new LearningObject();

        // Act
        learningObject.Name = name;

        // Assert
        learningObject.Name.Should().Be(name);
    }

    [Fact]
    public void Metadata_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var learningObject = new LearningObject();
        var expectedMetadata = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        learningObject.Metadata = expectedMetadata;

        // Assert
        learningObject.Metadata.Should().BeEquivalentTo(expectedMetadata);
    }

    [Fact]
    public void Metadata_AddItems_UpdatesCollection()
    {
        // Arrange
        var learningObject = new LearningObject();

        // Act
        learningObject.Metadata["author"] = "John Doe";
        learningObject.Metadata["version"] = "1.0";
        learningObject.Metadata["tags"] = "education,learning";

        // Assert
        learningObject.Metadata.Should().HaveCount(3);
        learningObject.Metadata["author"].Should().Be("John Doe");
        learningObject.Metadata["version"].Should().Be("1.0");
        learningObject.Metadata["tags"].Should().Be("education,learning");
    }

    [Fact]
    public void Metadata_ModifyExistingItems_UpdatesCorrectly()
    {
        // Arrange
        var learningObject = new LearningObject();
        learningObject.Metadata["key1"] = "original value";

        // Act
        learningObject.Metadata["key1"] = "updated value";

        // Assert
        learningObject.Metadata["key1"].Should().Be("updated value");
        learningObject.Metadata.Should().HaveCount(1);
    }

    [Fact]
    public void Metadata_RemoveItems_UpdatesCollection()
    {
        // Arrange
        var learningObject = new LearningObject();
        learningObject.Metadata["key1"] = "value1";
        learningObject.Metadata["key2"] = "value2";

        // Act
        learningObject.Metadata.Remove("key1");

        // Assert
        learningObject.Metadata.Should().HaveCount(1);
        learningObject.Metadata.Should().NotContainKey("key1");
        learningObject.Metadata["key2"].Should().Be("value2");
    }

    [Fact]
    public void Metadata_SetEmptyDictionary_ClearsMetadata()
    {
        // Arrange
        var learningObject = new LearningObject();
        learningObject.Metadata["key1"] = "value1";

        // Act
        learningObject.Metadata = new Dictionary<string, string>();

        // Assert
        learningObject.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Id_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var learningObject = new LearningObject();
        var expectedId = Guid.NewGuid();

        // Act
        learningObject.Id = expectedId;

        // Assert
        learningObject.Id.Should().Be(expectedId);
    }

    [Fact]
    public void InternalId_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var learningObject = new LearningObject();
        var expectedInternalId = 123;

        // Act
        learningObject.InternalId = expectedInternalId;

        // Assert
        learningObject.InternalId.Should().Be(expectedInternalId);
    }

    [Fact]
    public void LearningObject_InheritsFromBaseEntity()
    {
        // Arrange & Act
        var learningObject = new LearningObject();

        // Assert
        learningObject.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Metadata_HandleSpecialCharacters_StoresCorrectly()
    {
        // Arrange
        var learningObject = new LearningObject();
        var specialKey = "key-with_special.chars@123";
        var specialValue = "Value with spaces, punctuation! & symbols #123";

        // Act
        learningObject.Metadata[specialKey] = specialValue;

        // Assert
        learningObject.Metadata[specialKey].Should().Be(specialValue);
    }

    [Fact]
    public void Metadata_HandleEmptyAndNullValues_StoresCorrectly()
    {
        // Arrange
        var learningObject = new LearningObject();

        // Act
        learningObject.Metadata["empty"] = string.Empty;
        learningObject.Metadata["whitespace"] = "   ";

        // Assert
        learningObject.Metadata["empty"].Should().Be(string.Empty);
        learningObject.Metadata["whitespace"].Should().Be("   ");
        learningObject.Metadata.Should().HaveCount(2);
    }

    [Fact]
    public void Metadata_Casesensitivity_TreatsKeysAsCaseSensitive()
    {
        // Arrange
        var learningObject = new LearningObject();

        // Act
        learningObject.Metadata["Key"] = "value1";
        learningObject.Metadata["key"] = "value2";
        learningObject.Metadata["KEY"] = "value3";

        // Assert
        learningObject.Metadata.Should().HaveCount(3);
        learningObject.Metadata["Key"].Should().Be("value1");
        learningObject.Metadata["key"].Should().Be("value2");
        learningObject.Metadata["KEY"].Should().Be("value3");
    }
}
