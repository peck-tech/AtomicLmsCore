using AtomicLmsCore.Domain;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Persistence;
using AtomicLmsCore.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

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

    protected static Tenant CreateTestTenant(string name, string? slug = null, int? internalId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug ?? name.ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal),
            IsActive = true,
            Metadata = new Dictionary<string, string>(),
            InternalId = internalId ?? 0
        };

    public class GetByIdAsyncTests : TenantRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WhenTenantExists_ReturnsTenant()
        {
            var tenant = CreateTestTenant("Test Tenant", "test-tenant", 1);
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(tenant.Id);

            result.ShouldNotBeNull();
            result.Id.ShouldBe(tenant.Id);
            result.Name.ShouldBe(tenant.Name);
            result.Slug.ShouldBe(tenant.Slug);
            result.IsActive.ShouldBe(tenant.IsActive);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTenantNotFound_ReturnsNull()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await _repository.GetByIdAsync(nonExistentId);

            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WhenTenantIsDeleted_ReturnsNull()
        {
            var tenant = CreateTestTenant("Deleted Tenant", "deleted-tenant", 1);

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // Mark as deleted using reflection since IsDeleted is private setter
            var isDeletedProperty = typeof(BaseEntity).GetProperty("IsDeleted");
            isDeletedProperty?.SetValue(tenant, true);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(tenant.Id);

            result.ShouldBeNull();
        }
    }

    public class GetAllAsyncTests : TenantRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsOnlyNonDeletedTenants()
        {
            var tenant1 = CreateTestTenant("Tenant 1", "tenant-1", 1);
            var tenant2 = CreateTestTenant("Tenant 2", "tenant-2", 2);
            var deletedTenant = CreateTestTenant("Deleted Tenant", "deleted-tenant", 3);

            _context.Tenants.AddRange(tenant1, tenant2, deletedTenant);
            await _context.SaveChangesAsync();

            // Mark as deleted using reflection since IsDeleted is private setter
            var isDeletedProperty = typeof(BaseEntity).GetProperty("IsDeleted");
            isDeletedProperty?.SetValue(deletedTenant, true);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllAsync();

            result.Count.ShouldBe(2);
            result.ShouldContain(t => t.Id == tenant1.Id);
            result.ShouldContain(t => t.Id == tenant2.Id);
            result.ShouldNotContain(t => t.Id == deletedTenant.Id);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoTenants_ReturnsEmptyList()
        {
            var result = await _repository.GetAllAsync();

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }
    }

    public class AddAsyncTests : TenantRepositoryTests
    {
        [Fact]
        public async Task AddAsync_CreatesTenant()
        {
            var tenant = CreateTestTenant("New Tenant");

            var result = await _repository.AddAsync(tenant);

            result.ShouldNotBeNull();
            result.Id.ShouldNotBe(Guid.Empty);
            result.Name.ShouldBe("New Tenant");
            result.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            result.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));

            var savedTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == result.Id);
            savedTenant.ShouldNotBeNull();
        }

        [Fact]
        public async Task AddAsync_AssignsInternalId()
        {
            var tenant = CreateTestTenant("New Tenant");

            var result = await _repository.AddAsync(tenant);

            result.InternalId.ShouldBeGreaterThan(0);
        }
    }

    public class UpdateAsyncTests : TenantRepositoryTests
    {
        [Fact]
        public async Task UpdateAsync_WhenTenantExists_UpdatesTenant()
        {
            var tenant = CreateTestTenant("Original Name", "original-name", 1);
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            tenant.Name = "Updated Name";
            await _repository.UpdateAsync(tenant);

            var updatedTenant = await _context.Tenants.FirstAsync(t => t.Id == tenant.Id);
            updatedTenant.Name.ShouldBe("Updated Name");
            updatedTenant.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }
    }

    public class DeleteAsyncTests : TenantRepositoryTests
    {
        [Fact]
        public async Task DeleteAsync_WhenTenantExists_PerformsSoftDelete()
        {
            var tenant = CreateTestTenant("To Delete", "to-delete", 1);
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            var result = await _repository.DeleteAsync(tenant.Id);

            result.ShouldBeTrue();

            var deletedTenant = await _context.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == tenant.Id);

            deletedTenant.ShouldNotBeNull();
            deletedTenant.IsDeleted.ShouldBeTrue();
        }

        [Fact]
        public async Task DeleteAsync_WhenTenantNotFound_ReturnsFalse()
        {
            var nonExistentId = Guid.NewGuid();

            var result = await _repository.DeleteAsync(nonExistentId);

            result.ShouldBeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WhenTenantAlreadyDeleted_ReturnsFalse()
        {
            var tenant = CreateTestTenant("Already Deleted", "already-deleted", 1);

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // Mark as deleted first
            await _repository.DeleteAsync(tenant.Id);

            // Try to delete again
            var result = await _repository.DeleteAsync(tenant.Id);

            result.ShouldBeFalse();
        }
    }
}
