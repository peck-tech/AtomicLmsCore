using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Tenants.Services;
using AtomicLmsCore.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Application.Tests.Tenants.Services;

public class TenantServiceTests
{
    private readonly Mock<ITenantRepository> _repositoryMock;
    private readonly TenantService _service;

    public TenantServiceTests()
    {
        _repositoryMock = new();
        Mock<ILogger<TenantService>> loggerMock = new();
        _service = new(_repositoryMock.Object, loggerMock.Object);
    }

    public class GetByIdAsyncTests : TenantServiceTests
    {
        [Fact]
        public async Task GetByIdAsync_WhenTenantExists_ReturnsSuccessWithTenant()
        {
            var tenantId = Guid.NewGuid();
            var tenant = new Tenant { Id = tenantId, Name = "Test Tenant" };
            _repositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            var result = await _service.GetByIdAsync(tenantId);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(tenant);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTenantNotFound_ReturnsFailure()
        {
            var tenantId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tenant?)null);

            var result = await _service.GetByIdAsync(tenantId);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == $"Tenant with ID {tenantId} not found");
        }

        [Fact]
        public async Task GetByIdAsync_WhenExceptionThrown_ReturnsFailure()
        {
            var tenantId = Guid.NewGuid();
            var exception = new Exception("Database error");
            _repositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var result = await _service.GetByIdAsync(tenantId);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == $"An error occurred while retrieving the tenant: {exception.Message}");
        }
    }

    public class GetAllAsyncTests : TenantServiceTests
    {
        [Fact]
        public async Task GetAllAsync_WhenTenantsExist_ReturnsSuccessWithTenants()
        {
            var tenants = new List<Tenant>
            {
                new() { Id = Guid.NewGuid(), Name = "Tenant 1" }, new() { Id = Guid.NewGuid(), Name = "Tenant 2" }
            };
            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenants);

            var result = await _service.GetAllAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(tenants);
        }

        [Fact]
        public async Task GetAllAsync_WhenExceptionThrown_ReturnsFailure()
        {
            var exception = new Exception("Database error");
            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var result = await _service.GetAllAsync();

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == $"An error occurred while retrieving tenants: {exception.Message}");
        }
    }

    public class CreateAsyncTests : TenantServiceTests
    {
        [Fact]
        public async Task CreateAsync_WithValidName_ReturnsSuccessWithId()
        {
            var name = "New Tenant";
            var createdId = Guid.NewGuid();
            var createdTenant = new Tenant { Id = createdId, Name = name };
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdTenant);

            var result = await _service.CreateAsync(name);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(createdId);
            _repositoryMock.Verify(r => r.AddAsync(It.Is<Tenant>(t => t.Name == name), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNullName_ReturnsFailure()
        {
            var result = await _service.CreateAsync(null);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Tenant name is required");
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithEmptyName_ReturnsFailure()
        {
            var result = await _service.CreateAsync("");

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Tenant name is required");
        }

        [Fact]
        public async Task CreateAsync_WithWhitespaceName_ReturnsFailure()
        {
            var result = await _service.CreateAsync("   ");

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Tenant name is required");
        }

        [Fact]
        public async Task CreateAsync_WhenExceptionThrown_ReturnsFailure()
        {
            var name = "New Tenant";
            var exception = new Exception("Database error");
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var result = await _service.CreateAsync(name);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == $"An error occurred while creating the tenant: {exception.Message}");
        }
    }

    public class UpdateAsyncTests : TenantServiceTests
    {
        [Fact]
        public async Task UpdateAsync_WithValidNameAndExistingTenant_ReturnsSuccess()
        {
            var tenantId = Guid.NewGuid();
            var tenant = new Tenant { Id = tenantId, Name = "Original Name" };
            var updatedName = "Updated Name";

            _repositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            var result = await _service.UpdateAsync(tenantId, updatedName);

            result.IsSuccess.Should().BeTrue();
            tenant.Name.Should().Be(updatedName);
            _repositoryMock.Verify(r => r.UpdateAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNullName_ReturnsFailure()
        {
            var tenantId = Guid.NewGuid();

            var result = await _service.UpdateAsync(tenantId, null);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "Tenant name is required");
        }

        [Fact]
        public async Task UpdateAsync_WhenTenantNotFound_ReturnsFailure()
        {
            var tenantId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tenant)null);

            var result = await _service.UpdateAsync(tenantId, "Updated Name");

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == $"Tenant with ID {tenantId} not found");
        }

        [Fact]
        public async Task UpdateAsync_WhenExceptionThrown_ReturnsFailure()
        {
            var tenantId = Guid.NewGuid();
            var exception = new Exception("Database error");
            _repositoryMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var result = await _service.UpdateAsync(tenantId, "Updated Name");

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == $"An error occurred while updating the tenant: {exception.Message}");
        }
    }

    public class DeleteAsyncTests : TenantServiceTests
    {
        [Fact]
        public async Task DeleteAsync_WhenTenantExists_ReturnsSuccess()
        {
            var tenantId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.DeleteAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.DeleteAsync(tenantId);

            result.IsSuccess.Should().BeTrue();
            _repositoryMock.Verify(r => r.DeleteAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenTenantNotFound_ReturnsFailure()
        {
            var tenantId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.DeleteAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _service.DeleteAsync(tenantId);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == $"Tenant with ID {tenantId} not found");
        }

        [Fact]
        public async Task DeleteAsync_WhenExceptionThrown_ReturnsFailure()
        {
            var tenantId = Guid.NewGuid();
            var exception = new Exception("Database error");
            _repositoryMock.Setup(r => r.DeleteAsync(tenantId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var result = await _service.DeleteAsync(tenantId);

            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == $"An error occurred while deleting the tenant: {exception.Message}");
        }
    }
}
