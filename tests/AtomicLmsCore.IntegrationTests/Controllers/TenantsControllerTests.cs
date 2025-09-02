using System.Net;
using System.Text;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Infrastructure.Persistence;
using AtomicLmsCore.IntegrationTests.Common;
using AtomicLmsCore.WebApi.DTOs.Tenants;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AtomicLmsCore.IntegrationTests.Controllers;

public class TenantsControllerTests(IntegrationTestWebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAll_WithoutSuperAdminRole_ShouldReturnForbidden()
    {
        // Arrange - no role set (default user)

        // Act
        var response = await Client.GetAsync("/api/v0.1/solution/tenants");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_WithSuperAdminRole_ShouldReturnTenants()
    {
        // Arrange
        SetTestUserRole("superadmin");

        var tenant1 = new Tenant { Name = "Test Tenant 1", Slug = "test-tenant-1", DatabaseName = "TestDb1", IsActive = true };
        var tenant2 = new Tenant { Name = "Test Tenant 2", Slug = "test-tenant-2", DatabaseName = "TestDb2", IsActive = false };

        await SeedDatabase<SolutionsDbContext>(db =>
        {
            db.Tenants.AddRange(tenant1, tenant2);
        });

        // Act
        var response = await Client.GetAsync("/api/v0.1/solution/tenants");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var tenants = JsonConvert.DeserializeObject<List<TenantListDto>>(content);

        tenants.Should().NotBeNull();
        tenants.Should().HaveCount(2);
        tenants.Should().Contain(t => t.Name == "Test Tenant 1" && t.IsActive == true);
        tenants.Should().Contain(t => t.Name == "Test Tenant 2" && t.IsActive == false);
    }

    [Fact]
    public async Task GetById_WithSuperAdminRole_ExistingTenant_ShouldReturnTenant()
    {
        // Arrange
        SetTestUserRole("superadmin");

        var tenant = new Tenant { Name = "Test Tenant", Slug = "test-tenant", DatabaseName = "TestDb", IsActive = true, Metadata = new Dictionary<string, string> { ["Key"] = "Value" } };

        await SeedDatabase<SolutionsDbContext>(db =>
        {
            db.Tenants.Add(tenant);
        });

        // Act
        var response = await Client.GetAsync($"/api/v0.1/solution/tenants/{tenant.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var tenantDto = JsonConvert.DeserializeObject<TenantDto>(content);

        tenantDto.Should().NotBeNull();
        tenantDto!.Id.Should().Be(tenant.Id);
        tenantDto.Name.Should().Be("Test Tenant");
        tenantDto.Slug.Should().Be("test-tenant");
        tenantDto.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WithSuperAdminRole_NonExistingTenant_ShouldReturnNotFound()
    {
        // Arrange
        SetTestUserRole("superadmin");
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v0.1/solution/tenants/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithSuperAdminRole_ValidRequest_ShouldCreateTenant()
    {
        // Arrange
        SetTestUserRole("superadmin");

        var createRequest = new CreateTenantRequestDto(
            "New Tenant",
            "new-tenant",
            "NewTenantDb",
            true,
            new Dictionary<string, string> { ["Environment"] = "Test" }
        );

        var json = JsonConvert.SerializeObject(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v0.1/solution/tenants", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdTenantId = JsonConvert.DeserializeObject<Guid>(responseContent);
        createdTenantId.Should().NotBeEmpty();

        // Verify tenant was created in database
        await using var dbContext = GetDbContext<SolutionsDbContext>();
        var createdTenant = await dbContext.Tenants.FindAsync(createdTenantId);
        createdTenant.Should().NotBeNull();
        createdTenant!.Name.Should().Be("New Tenant");
        createdTenant.Slug.Should().Be("new-tenant");
        createdTenant.DatabaseName.Should().Be("NewTenantDb");
        createdTenant.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithoutSuperAdminRole_ShouldReturnForbidden()
    {
        // Arrange - no role set
        var createRequest = new CreateTenantRequestDto(
            "New Tenant",
            "new-tenant",
            "NewTenantDb",
            true
        );

        var json = JsonConvert.SerializeObject(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v0.1/solution/tenants", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        SetTestUserRole("superadmin");

        var createRequest = new CreateTenantRequestDto(
            "", // Invalid - empty name
            "test-slug",
            "TestDb",
            true
        );

        var json = JsonConvert.SerializeObject(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v0.1/solution/tenants", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithSuperAdminRole_ValidRequest_ShouldUpdateTenant()
    {
        // Arrange
        SetTestUserRole("superadmin");

        var tenant = new Tenant { Name = "Original Name", Slug = "original-slug", DatabaseName = "OriginalDb", IsActive = true };

        await SeedDatabase<SolutionsDbContext>(db =>
        {
            db.Tenants.Add(tenant);
        });

        var updateRequest = new UpdateTenantRequestDto(
            "Updated Name",
            "updated-slug",
            false,
            new Dictionary<string, string> { ["Updated"] = "true" }
        );

        var json = JsonConvert.SerializeObject(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/v0.1/solution/tenants/{tenant.Id}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify tenant was updated in database
        await using var dbContext = GetDbContext<SolutionsDbContext>();
        var updatedTenant = await dbContext.Tenants.FindAsync(tenant.Id);
        updatedTenant.Should().NotBeNull();
        updatedTenant!.Name.Should().Be("Updated Name");
        updatedTenant.Slug.Should().Be("updated-slug");
        updatedTenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Update_NonExistingTenant_ShouldReturnNotFound()
    {
        // Arrange
        SetTestUserRole("superadmin");
        var nonExistingId = Guid.NewGuid();

        var updateRequest = new UpdateTenantRequestDto(
            "Updated Name",
            "updated-slug",
            false,
            null
        );

        var json = JsonConvert.SerializeObject(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/v0.1/solution/tenants/{nonExistingId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithSuperAdminRole_ExistingTenant_ShouldDeleteTenant()
    {
        // Arrange
        SetTestUserRole("superadmin");

        var tenant = new Tenant { Name = "Test Tenant", Slug = "test-tenant", DatabaseName = "TestDb", IsActive = true };

        await SeedDatabase<SolutionsDbContext>(db =>
        {
            db.Tenants.Add(tenant);
        });

        // Act
        var response = await Client.DeleteAsync($"/api/v0.1/solution/tenants/{tenant.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify tenant was soft deleted in database
        using var dbContext = GetDbContext<SolutionsDbContext>();
        var deletedTenant = await dbContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenant.Id);
        deletedTenant.Should().NotBeNull();
        deletedTenant!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_NonExistingTenant_ShouldReturnNotFound()
    {
        // Arrange
        SetTestUserRole("superadmin");
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v0.1/solution/tenants/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithoutSuperAdminRole_ShouldReturnForbidden()
    {
        // Arrange - no role set
        var tenantId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v0.1/solution/tenants/{tenantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
