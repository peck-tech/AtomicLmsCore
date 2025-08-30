using AtomicLmsCore.Domain;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Persistence;
using AtomicLmsCore.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Infrastructure.Tests.Persistence.Repositories;

public class TenantRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IIdGenerator> _idGeneratorMock;
    private readonly Mock<ILogger<TenantRepository>> _loggerMock;
    private readonly TenantRepository _repository;

    public TenantRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _idGeneratorMock = new();
        _idGeneratorMock.Setup(x => x.NewId()).Returns(() => Guid.NewGuid());

        _context = new(options, _idGeneratorMock.Object);
        _loggerMock = new();
        _repository = new(_context);
    }

    public void Dispose() => _context.Dispose();

    public class GetByIdAsyncTests : TenantRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WhenTenantExists_ReturnsTenant()
        {
            var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Test Tenant", InternalId = 1 };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(tenant.Id);

            result.Should().NotBeNull();
            result.Id.Should().Be(tenant.Id);
            result.Name.Should().Be(tenant.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTenantNotFound_ReturnsNull()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await _repository.GetByIdAsync(nonExistentId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WhenTenantIsDeleted_ReturnsNull()
        {
            var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Deleted Tenant", InternalId = 1 };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // Mark as deleted using reflection since IsDeleted is private setter
            var isDeletedProperty = typeof(BaseEntity).GetProperty("IsDeleted");
            isDeletedProperty?.SetValue(tenant, true);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(tenant.Id);

            result.Should().BeNull();
        }
    }

    public class GetAllAsyncTests : TenantRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsOnlyNonDeletedTenants()
        {
            var tenant1 = new Tenant { Id = Guid.NewGuid(), Name = "Tenant 1", InternalId = 1 };
            var tenant2 = new Tenant { Id = Guid.NewGuid(), Name = "Tenant 2", InternalId = 2 };
            var deletedTenant = new Tenant { Id = Guid.NewGuid(), Name = "Deleted Tenant", InternalId = 3 };

            _context.Tenants.AddRange(tenant1, tenant2, deletedTenant);
            await _context.SaveChangesAsync();

            // Mark as deleted using reflection since IsDeleted is private setter
            var isDeletedProperty = typeof(BaseEntity).GetProperty("IsDeleted");
            isDeletedProperty?.SetValue(deletedTenant, true);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllAsync();

            result.Should().HaveCount(2);
            result.Should().Contain(t => t.Id == tenant1.Id);
            result.Should().Contain(t => t.Id == tenant2.Id);
            result.Should().NotContain(t => t.Id == deletedTenant.Id);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoTenants_ReturnsEmptyList()
        {
            var result = await _repository.GetAllAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }

    public class AddAsyncTests : TenantRepositoryTests
    {
        [Fact]
        public async Task AddAsync_CreatesTenant()
        {
            var tenant = new Tenant { Name = "New Tenant" };

            var result = await _repository.AddAsync(tenant);

            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Name.Should().Be("New Tenant");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            var savedTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == result.Id);
            savedTenant.Should().NotBeNull();
        }

        [Fact]
        public async Task AddAsync_AssignsInternalId()
        {
            var tenant = new Tenant { Name = "New Tenant" };

            var result = await _repository.AddAsync(tenant);

            result.InternalId.Should().BeGreaterThan(0);
        }
    }

    public class UpdateAsyncTests : TenantRepositoryTests
    {
        [Fact]
        public async Task UpdateAsync_WhenTenantExists_UpdatesTenant()
        {
            var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Original Name", InternalId = 1 };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            tenant.Name = "Updated Name";
            await _repository.UpdateAsync(tenant);

            var updatedTenant = await _context.Tenants.FirstAsync(t => t.Id == tenant.Id);
            updatedTenant.Name.Should().Be("Updated Name");
            updatedTenant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }

    public class DeleteAsyncTests : TenantRepositoryTests
    {
        [Fact]
        public async Task DeleteAsync_WhenTenantExists_PerformsSoftDelete()
        {
            var tenant = new Tenant { Id = Guid.NewGuid(), Name = "To Delete", InternalId = 1 };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            var result = await _repository.DeleteAsync(tenant.Id);

            result.Should().BeTrue();

            var deletedTenant = await _context.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == tenant.Id);

            deletedTenant.Should().NotBeNull();
            deletedTenant.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_WhenTenantNotFound_ReturnsFalse()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await _repository.DeleteAsync(nonExistentId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WhenTenantAlreadyDeleted_ReturnsFalse()
        {
            var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Already Deleted", InternalId = 1 };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // Mark as deleted first
            await _repository.DeleteAsync(tenant.Id);

            // Try to delete again
            var result = await _repository.DeleteAsync(tenant.Id);

            result.Should().BeFalse();
        }
    }
}
