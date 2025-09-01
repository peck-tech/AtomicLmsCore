using System.Text.Json;
using AtomicLmsCore.WebApi.Common;
using FluentAssertions;

namespace AtomicLmsCore.WebApi.Tests.Common;

public class ErrorResponseDtoTests
{
    [Fact]
    public void ValidationError_CreatesCorrectErrorResponse()
    {
        // Arrange
        var errors = new List<string> { "Field is required", "Invalid format" };
        const string CorrelationId = "test-correlation-id";

        // Act
        var result = ErrorResponseDto.ValidationError(errors, CorrelationId);

        // Assert
        result.Type.Should().Be("Validation");
        result.Title.Should().Be("One or more validation errors occurred.");
        result.Status.Should().Be(400);
        result.Errors.Should().BeEquivalentTo(errors);
        result.CorrelationId.Should().Be(CorrelationId);
    }

    [Fact]
    public void ValidationError_WithoutCorrelationId_ReturnsNullCorrelationId()
    {
        // Arrange
        var errors = new List<string> { "Test error" };

        // Act
        var result = ErrorResponseDto.ValidationError(errors);

        // Assert
        result.CorrelationId.Should().BeNull();
    }

    [Fact]
    public void BusinessError_CreatesCorrectErrorResponse()
    {
        // Arrange
        var errors = new List<string> { "Business rule violated" };
        const string CorrelationId = "business-error-id";

        // Act
        var result = ErrorResponseDto.BusinessError(errors, CorrelationId);

        // Assert
        result.Type.Should().Be("Business");
        result.Title.Should().Be("Business rule violation.");
        result.Status.Should().Be(400);
        result.Errors.Should().BeEquivalentTo(errors);
        result.CorrelationId.Should().Be(CorrelationId);
    }

    [Fact]
    public void SystemError_CreatesCorrectErrorResponse()
    {
        // Arrange
        const string CorrelationId = "system-error-id";

        // Act
        var result = ErrorResponseDto.SystemError(CorrelationId);

        // Assert
        result.Type.Should().Be("System");
        result.Title.Should().Be("An internal server error occurred.");
        result.Status.Should().Be(500);
        result.Errors.Should().ContainSingle("An error occurred processing your request.");
        result.CorrelationId.Should().Be(CorrelationId);
    }

    [Fact]
    public void NotFoundError_CreatesCorrectErrorResponse()
    {
        // Arrange
        const string Resource = "User";
        const string CorrelationId = "not-found-id";

        // Act
        var result = ErrorResponseDto.NotFoundError(Resource, CorrelationId);

        // Assert
        result.Type.Should().Be("NotFound");
        result.Title.Should().Be("Resource not found.");
        result.Status.Should().Be(404);
        result.Errors.Should().ContainSingle("User not found.");
        result.CorrelationId.Should().Be(CorrelationId);
    }

    [Fact]
    public void BadRequestError_CreatesCorrectErrorResponse()
    {
        // Arrange
        const string Message = "Invalid request format";
        const string CorrelationId = "bad-request-id";

        // Act
        var result = ErrorResponseDto.BadRequestError(Message, CorrelationId);

        // Assert
        result.Type.Should().Be("BadRequest");
        result.Title.Should().Be("Bad request.");
        result.Status.Should().Be(400);
        result.Errors.Should().ContainSingle(Message);
        result.CorrelationId.Should().Be(CorrelationId);
    }

    [Fact]
    public void ForbiddenError_CreatesCorrectErrorResponse()
    {
        // Arrange
        const string Message = "Access denied to resource";
        const string CorrelationId = "forbidden-id";

        // Act
        var result = ErrorResponseDto.ForbiddenError(Message, CorrelationId);

        // Assert
        result.Type.Should().Be("Forbidden");
        result.Title.Should().Be("Access denied.");
        result.Status.Should().Be(403);
        result.Errors.Should().ContainSingle(Message);
        result.CorrelationId.Should().Be(CorrelationId);
    }

    [Theory]
    [InlineData("ValidationError", 400)]
    [InlineData("BusinessError", 400)]
    [InlineData("SystemError", 500)]
    [InlineData("NotFoundError", 404)]
    [InlineData("BadRequestError", 400)]
    [InlineData("ForbiddenError", 403)]
    public void ErrorTypes_HaveCorrectStatusCodes(string errorType, int expectedStatus)
    {
        // Arrange & Act
        var result = errorType switch
        {
            "ValidationError" => ErrorResponseDto.ValidationError(["Test"]),
            "BusinessError" => ErrorResponseDto.BusinessError(["Test"]),
            "SystemError" => ErrorResponseDto.SystemError(),
            "NotFoundError" => ErrorResponseDto.NotFoundError("Resource"),
            "BadRequestError" => ErrorResponseDto.BadRequestError("Test"),
            "ForbiddenError" => ErrorResponseDto.ForbiddenError("Test"),
            _ => throw new ArgumentException("Unknown error type")
        };

        // Assert
        result.Status.Should().Be(expectedStatus);
    }

    [Fact]
    public void ErrorResponseDto_SerializesToJson_WithCamelCaseProperties()
    {
        // Arrange
        var errorResponse = ErrorResponseDto.ValidationError(
            ["Test error"],
            "test-correlation");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var json = JsonSerializer.Serialize(errorResponse, options);

        // Assert
        json.Should().Contain("\"type\":");
        json.Should().Contain("\"title\":");
        json.Should().Contain("\"status\":");
        json.Should().Contain("\"errors\":");
        json.Should().Contain("\"correlationId\":");

        // Should not contain Pascal case
        json.Should().NotContain("\"Type\":");
        json.Should().NotContain("\"Title\":");
    }

    [Fact]
    public void ErrorResponseDto_DeserializesFromJson_Correctly()
    {
        // Arrange
        var originalDto = ErrorResponseDto.ValidationError(
            ["Error 1", "Error 2"],
            "test-id");

        var json = JsonSerializer.Serialize(originalDto);

        // Act
        var result = JsonSerializer.Deserialize<ErrorResponseDto>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be("Validation");
        result.Title.Should().Be("One or more validation errors occurred.");
        result.Status.Should().Be(400);
        result.Errors.Should().BeEquivalentTo(["Error 1", "Error 2"]);
        result.CorrelationId.Should().Be("test-id");
    }

    [Fact]
    public void ErrorResponseDto_WithEmptyErrorsList_IsValid()
    {
        // Act
        var result = ErrorResponseDto.ValidationError([]);

        // Assert
        result.Errors.Should().BeEmpty();
        result.Type.Should().Be("Validation");
    }

    [Fact]
    public void ErrorResponseDto_WithLongErrorMessages_HandlesCorrectly()
    {
        // Arrange
        var longError = new string('A', 1000);
        var errors = new List<string> { longError };

        // Act
        var result = ErrorResponseDto.ValidationError(errors);

        // Assert
        result.Errors.Should().ContainSingle(longError);
    }
}
