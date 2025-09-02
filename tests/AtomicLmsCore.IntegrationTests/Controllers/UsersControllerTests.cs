using System.Net;
using System.Text;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Infrastructure.Persistence;
using AtomicLmsCore.IntegrationTests.Common;
using AtomicLmsCore.WebApi.DTOs.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AtomicLmsCore.IntegrationTests.Controllers;

public class UsersControllerTests(IntegrationTestWebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    private static readonly Guid TestTenantId = Guid.NewGuid();

    [Fact]
    public async Task GetAll_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Clear(); // Remove authentication

        // Act
        var response = await Client.GetAsync("/api/v0.1/learners/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_WithoutTenantAccess_ShouldReturnForbidden()
    {
        // Arrange - authenticated but no tenant claims
        SetTenantHeader(TestTenantId);

        // Act
        var response = await Client.GetAsync("/api/v0.1/learners/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_WithValidTenantAccess_ShouldReturnUsers()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var user1 = new User { ExternalUserId = "ext-user-1", Email = "user1@test.com", FirstName = "John", LastName = "Doe", DisplayName = "John Doe", IsActive = true };
        var user2 = new User { ExternalUserId = "ext-user-2", Email = "user2@test.com", FirstName = "Jane", LastName = "Smith", DisplayName = "Jane Smith", IsActive = false };

        await SeedDatabase<TenantDbContext>(db =>
        {
            db.Users.AddRange(user1, user2);
        });

        // Act
        var response = await Client.GetAsync("/api/v0.1/learners/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var users = JsonConvert.DeserializeObject<List<UserListDto>>(content);

        users.Should().NotBeNull();
        users.Should().HaveCount(2);
        users.Should().Contain(u => u.Email == "user1@test.com" && u.IsActive == true);
        users.Should().Contain(u => u.Email == "user2@test.com" && u.IsActive == false);
    }

    [Fact]
    public async Task GetById_WithValidTenantAccess_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var user = new User { ExternalUserId = "ext-user-1", Email = "user@test.com", FirstName = "John", LastName = "Doe", DisplayName = "John Doe", IsActive = true, Metadata = new Dictionary<string, string> { ["Role"] = "Student" } };

        await SeedDatabase<TenantDbContext>(db =>
        {
            db.Users.Add(user);
        });

        // Act
        var response = await Client.GetAsync($"/api/v0.1/learners/users/{user.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var userDto = JsonConvert.DeserializeObject<UserDto>(content);

        userDto.Should().NotBeNull();
        userDto!.Id.Should().Be(user.Id);
        userDto.Email.Should().Be("user@test.com");
        userDto.FirstName.Should().Be("John");
        userDto.LastName.Should().Be("Doe");
        userDto.DisplayName.Should().Be("John Doe");
        userDto.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WithValidTenantAccess_NonExistingUser_ShouldReturnNotFound()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v0.1/learners/users/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidTenantAccess_ValidRequest_ShouldCreateUser()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var createRequest = new CreateUserRequestDto
        {
            ExternalUserId = "ext-new-user",
            Email = "newuser@test.com",
            FirstName = "New",
            LastName = "User",
            DisplayName = "New User",
            IsActive = true,
            Metadata = new Dictionary<string, string> { ["Department"] = "IT" }
        };

        var json = JsonConvert.SerializeObject(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v0.1/learners/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdUserId = JsonConvert.DeserializeObject<Guid>(responseContent);
        createdUserId.Should().NotBeEmpty();

        // Verify user was created in database
        using var dbContext = GetDbContext<TenantDbContext>();
        var createdUser = await dbContext.Users.FindAsync(createdUserId);
        createdUser.Should().NotBeNull();
        createdUser!.ExternalUserId.Should().Be("ext-new-user");
        createdUser.Email.Should().Be("newuser@test.com");
        createdUser.FirstName.Should().Be("New");
        createdUser.LastName.Should().Be("User");
        createdUser.DisplayName.Should().Be("New User");
        createdUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithoutTenantAccess_ShouldReturnForbidden()
    {
        // Arrange - no tenant claims
        var createRequest = new CreateUserRequestDto
        {
            ExternalUserId = "ext-new-user",
            Email = "newuser@test.com",
            FirstName = "New",
            LastName = "User",
            DisplayName = "New User",
            IsActive = true
        };

        var json = JsonConvert.SerializeObject(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v0.1/learners/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var createRequest = new CreateUserRequestDto
        {
            ExternalUserId = "", // Invalid - empty external user ID
            Email = "invalid-email", // Invalid email format
            FirstName = "Test",
            LastName = "User",
            DisplayName = "Test User",
            IsActive = true
        };

        var json = JsonConvert.SerializeObject(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v0.1/learners/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidTenantAccess_ValidRequest_ShouldUpdateUser()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var user = new User { ExternalUserId = "ext-user-1", Email = "original@test.com", FirstName = "Original", LastName = "Name", DisplayName = "Original Name", IsActive = true };

        await SeedDatabase<TenantDbContext>(db =>
        {
            db.Users.Add(user);
        });

        var updateRequest = new UpdateUserRequestDto
        {
            Email = "updated@test.com",
            FirstName = "Updated",
            LastName = "User",
            DisplayName = "Updated User",
            IsActive = false,
            Metadata = new Dictionary<string, string> { ["Updated"] = "true" }
        };

        var json = JsonConvert.SerializeObject(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/v0.1/learners/users/{user.Id}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user was updated in database
        using var dbContext = GetDbContext<TenantDbContext>();
        var updatedUser = await dbContext.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.Email.Should().Be("updated@test.com");
        updatedUser.FirstName.Should().Be("Updated");
        updatedUser.LastName.Should().Be("User");
        updatedUser.DisplayName.Should().Be("Updated User");
        updatedUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Update_NonExistingUser_ShouldReturnNotFound()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);
        var nonExistingId = Guid.NewGuid();

        var updateRequest = new UpdateUserRequestDto
        {
            Email = "updated@test.com",
            FirstName = "Updated",
            LastName = "User",
            DisplayName = "Updated User",
            IsActive = false
        };

        var json = JsonConvert.SerializeObject(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/v0.1/learners/users/{nonExistingId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithValidTenantAccess_ExistingUser_ShouldDeleteUser()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);

        var user = new User { ExternalUserId = "ext-user-1", Email = "user@test.com", FirstName = "Test", LastName = "User", DisplayName = "Test User", IsActive = true };

        await SeedDatabase<TenantDbContext>(db =>
        {
            db.Users.Add(user);
        });

        // Act
        var response = await Client.DeleteAsync($"/api/v0.1/learners/users/{user.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user was soft deleted in database
        using var dbContext = GetDbContext<TenantDbContext>();
        var deletedUser = await dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == user.Id);
        deletedUser.Should().NotBeNull();
        deletedUser!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_NonExistingUser_ShouldReturnNotFound()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(TestTenantId);
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v0.1/learners/users/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithoutTenantAccess_ShouldReturnForbidden()
    {
        // Arrange - no tenant claims
        var userId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v0.1/learners/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AllEndpoints_WithMismatchedTenantHeader_ShouldHandleGracefully()
    {
        // Arrange
        SetTestUserTenant(TestTenantId);
        SetTenantHeader(Guid.NewGuid()); // Different tenant ID in header

        // Act & Assert - Should handle tenant mismatch
        var getResponse = await Client.GetAsync("/api/v0.1/learners/users");
        getResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
    }
}
