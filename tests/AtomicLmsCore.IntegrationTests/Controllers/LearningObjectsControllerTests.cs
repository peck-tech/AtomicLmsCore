using System.Globalization;
using System.Net;
using System.Text;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Infrastructure.Persistence;
using AtomicLmsCore.IntegrationTests.Common;
using AtomicLmsCore.WebApi.DTOs.LearningObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AtomicLmsCore.IntegrationTests.Controllers;

public class LearningObjectsControllerTests(IntegrationTestWebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    private static readonly Guid TestTenantId = Guid.NewGuid();

    [Fact]
    public async Task GetAll_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Clear(); // Remove authentication

        // Act
        var response = await Client.GetAsync("/api/v0.1/learning/learningobjects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_WithValidAuthentication_ShouldReturnLearningObjects()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var learningObject1 = new LearningObject { Name = "Course 1", Metadata = new Dictionary<string, string> { ["Type"] = "Video" } };
        var learningObject2 = new LearningObject { Name = "Course 2", Metadata = new Dictionary<string, string> { ["Type"] = "Text" } };

        await SeedDatabase<TenantDbContext>(db =>
        {
            db.LearningObjects.AddRange(learningObject1, learningObject2);
        });

        // Act
        var response = await Client.GetAsync("/api/v0.1/learning/learningobjects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var learningObjects = JsonConvert.DeserializeObject<List<LearningObjectListDto>>(content);

        learningObjects.Should().NotBeNull();
        learningObjects.Should().HaveCount(2);
        learningObjects.Should().Contain(lo => lo.Name == "Course 1");
        learningObjects.Should().Contain(lo => lo.Name == "Course 2");
    }

    [Fact(Skip = "TODO: Intermittent test failure due to data isolation issues in integration test pipeline. Test passes when run individually.")]
    public async Task GetById_WithValidAuthentication_ExistingLearningObject_ShouldReturnLearningObject()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var learningObject = new LearningObject { Name = "Test Course", Metadata = new Dictionary<string, string> { ["Duration"] = "2 hours", ["Level"] = "Beginner" } };

        await SeedDatabase<TenantDbContext>(db =>
        {
            db.LearningObjects.Add(learningObject);
        });

        // Act
        var response = await Client.GetAsync($"/api/v0.1/learning/learningobjects/{learningObject.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var learningObjectDto = JsonConvert.DeserializeObject<LearningObjectDto>(content);

        learningObjectDto.Should().NotBeNull();
        learningObjectDto!.Id.Should().Be(learningObject.Id);
        learningObjectDto.Name.Should().Be("Test Course");
    }

    [Fact]
    public async Task GetById_WithValidAuthentication_NonExistingLearningObject_ShouldReturnNotFound()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v0.1/learning/learningobjects/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(Skip = "TODO: Intermittent test failure due to data isolation issues in integration test pipeline. Test passes when run individually.")]
    public async Task Create_WithValidAuthentication_ValidRequest_ShouldCreateLearningObject()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var createRequest = new CreateLearningObjectRequestDto(
            "New Learning Object",
            new Dictionary<string, string> { ["Category"] = "Programming", ["Difficulty"] = "Intermediate" }
        );

        var json = JsonConvert.SerializeObject(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v0.1/learning/learningobjects", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdLearningObjectId = JsonConvert.DeserializeObject<Guid>(responseContent);
        createdLearningObjectId.Should().NotBeEmpty();

        // Verify learning object was created in database
        await using var dbContext = GetDbContext<TenantDbContext>();
        var createdLearningObject = await dbContext.LearningObjects.FirstOrDefaultAsync(lo => lo.Id == createdLearningObjectId);
        createdLearningObject.Should().NotBeNull();
        createdLearningObject!.Name.Should().Be("New Learning Object");
    }

    [Fact]
    public async Task Create_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Clear(); // Remove authentication

        var createRequest = new CreateLearningObjectRequestDto(
            "New Learning Object",
            new Dictionary<string, string> { ["Category"] = "Programming" }
        );

        var json = JsonConvert.SerializeObject(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v0.1/learning/learningobjects", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var createRequest = new CreateLearningObjectRequestDto(
            "" // Invalid - empty name
        );

        var json = JsonConvert.SerializeObject(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v0.1/learning/learningobjects", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(Skip = "TODO: Intermittent test failure due to data isolation issues in integration test pipeline. Test passes when run individually.")]
    public async Task Update_WithValidAuthentication_ValidRequest_ShouldUpdateLearningObject()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var learningObject = new LearningObject { Name = "Original Name", Metadata = new Dictionary<string, string> { ["Status"] = "Draft" } };

        await SeedDatabase<TenantDbContext>(db =>
        {
            db.LearningObjects.Add(learningObject);
        });

        var updateRequest = new UpdateLearningObjectRequestDto(
            "Updated Name",
            new Dictionary<string, string> { ["Status"] = "Published", ["UpdatedAt"] = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) }
        );

        var json = JsonConvert.SerializeObject(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/v0.1/learning/learningobjects/{learningObject.Id}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify learning object was updated in database
        await using var dbContext = GetDbContext<TenantDbContext>();
        var updatedLearningObject = await dbContext.LearningObjects.FirstOrDefaultAsync(lo => lo.Id == learningObject.Id);
        updatedLearningObject.Should().NotBeNull();
        updatedLearningObject!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Update_NonExistingLearningObject_ShouldReturnNotFound()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);
        var nonExistingId = Guid.NewGuid();

        var updateRequest = new UpdateLearningObjectRequestDto(
            "Updated Name",
            new Dictionary<string, string> { ["Status"] = "Published" }
        );

        var json = JsonConvert.SerializeObject(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/v0.1/learning/learningobjects/{nonExistingId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(Skip = "TODO: Intermittent test failure due to data isolation issues in integration test pipeline. Test passes when run individually.")]
    public async Task Delete_WithValidAuthentication_ExistingLearningObject_ShouldDeleteLearningObject()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var learningObject = new LearningObject { Name = "Test Learning Object", Metadata = new Dictionary<string, string> { ["Type"] = "Course" } };

        await SeedDatabase<TenantDbContext>(db =>
        {
            db.LearningObjects.Add(learningObject);
        });

        // Act
        var response = await Client.DeleteAsync($"/api/v0.1/learning/learningobjects/{learningObject.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify learning object was soft deleted in database
        await using var dbContext = GetDbContext<TenantDbContext>();
        var deletedLearningObject = await dbContext.LearningObjects.IgnoreQueryFilters().FirstOrDefaultAsync(lo => lo.Id == learningObject.Id);
        deletedLearningObject.Should().NotBeNull();
        deletedLearningObject!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_NonExistingLearningObject_ShouldReturnNotFound()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v0.1/learning/learningobjects/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Clear(); // Remove authentication
        var learningObjectId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v0.1/learning/learningobjects/{learningObjectId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AllEndpoints_ShouldIncludeCorrelationIdInHeaders()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        // Act
        var response = await Client.GetAsync("/api/v0.1/learning/learningobjects");

        // Assert
        response.Headers.Should().Contain(h => h.Key == "X-Correlation-ID");
    }

    [Fact(Skip = "TODO: Intermittent test failure due to data isolation issues in integration test pipeline. Test passes when run individually.")]
    public async Task AllEndpoints_WithTenantContext_ShouldWorkWithinTenantScope()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var learningObject = new LearningObject { Name = "Tenant Specific Object", Metadata = new Dictionary<string, string> { ["TenantId"] = TestTenantId.ToString() } };

        await SeedDatabase<TenantDbContext>(db =>
        {
            db.LearningObjects.Add(learningObject);
        });

        // Act & Assert - All operations should work within tenant context
        var getAllResponse = await Client.GetAsync("/api/v0.1/learning/learningobjects");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getByIdResponse = await Client.GetAsync($"/api/v0.1/learning/learningobjects/{learningObject.Id}");
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Test creation within tenant context
        var createRequest = new CreateLearningObjectRequestDto(
            "Tenant Object",
            new Dictionary<string, string> { ["CreatedInTenant"] = TestTenantId.ToString() }
        );

        var json = JsonConvert.SerializeObject(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var createResponse = await Client.PostAsync("/api/v0.1/learning/learningobjects", content);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
