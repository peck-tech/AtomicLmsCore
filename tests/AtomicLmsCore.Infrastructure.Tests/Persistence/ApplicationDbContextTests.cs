using AtomicLmsCore.Domain;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;

namespace AtomicLmsCore.Infrastructure.Tests.Persistence;

public class ApplicationDbContextTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Guid _generatedId = Guid.NewGuid();
    private readonly Mock<IIdGenerator> _idGeneratorMock;

    public ApplicationDbContextTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _idGeneratorMock = new();
        _idGeneratorMock.Setup(x => x.NewId()).Returns(_generatedId);

        _context = new(options, _idGeneratorMock.Object);
    }

    public void Dispose() => _context.Dispose();

    private static Tenant CreateTestTenant(string name, string? slug = null) =>
        new()
        {
            Name = name,
            Slug = slug ?? name.ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal),
            IsActive = true,
            Metadata = new Dictionary<string, string>()
        };

    [Fact]
    public async Task SaveChangesAsync_SetsCreatedAtAndUpdatedAt_ForNewEntities()
    {
        var tenant = CreateTestTenant("New Tenant");
        _context.Tenants.Add(tenant);

        await _context.SaveChangesAsync();

        tenant.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        tenant.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        (tenant.CreatedAt - tenant.UpdatedAt).Duration().ShouldBeLessThan(TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task SaveChangesAsync_SetsId_ForNewEntitiesWithoutId()
    {
        var tenant = CreateTestTenant("New Tenant");
        tenant.Id.ShouldBe(Guid.Empty);

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        tenant.Id.ShouldBe(_generatedId);
        _idGeneratorMock.Verify(x => x.NewId(), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_DoesNotOverrideId_ForEntitiesWithExistingId()
    {
        var existingId = Guid.NewGuid();
        var tenant = CreateTestTenant("New Tenant");
        tenant.Id = existingId;

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        tenant.Id.ShouldBe(existingId);
        _idGeneratorMock.Verify(x => x.NewId(), Times.Never);
    }

    [Fact]
    public async Task SaveChangesAsync_UpdatesUpdatedAt_ForModifiedEntities()
    {
        var tenant = CreateTestTenant("Original Name");
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        var originalUpdatedAt = tenant.UpdatedAt;
        await Task.Delay(10);

        tenant.Name = "Updated Name";
        _context.Entry(tenant).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        tenant.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
        tenant.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        tenant.CreatedAt.ShouldBe(tenant.CreatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_DoesNotUpdateCreatedAt_ForModifiedEntities()
    {
        var tenant = CreateTestTenant("Original Name");
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        var originalCreatedAt = tenant.CreatedAt;

        tenant.Name = "Updated Name";
        _context.Entry(tenant).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        tenant.CreatedAt.ShouldBe(originalCreatedAt);
    }

    [Fact]
    public async Task QueryFilter_ExcludesDeletedEntities()
    {
        var activeTenant = CreateTestTenant("Active Tenant");
        activeTenant.Id = Guid.NewGuid();
        var deletedTenant = CreateTestTenant("Deleted Tenant");
        deletedTenant.Id = Guid.NewGuid();

        // Mark as deleted using reflection since IsDeleted is private setter
        var isDeletedProperty = typeof(BaseEntity).GetProperty("IsDeleted");
        isDeletedProperty?.SetValue(deletedTenant, true);

        _context.Tenants.AddRange(activeTenant, deletedTenant);
        await _context.SaveChangesAsync();

        var tenants = await _context.Tenants.ToListAsync();

        // Query filter should exclude deleted entities
        tenants.Count.ShouldBe(1);
        tenants.ShouldContain(t => t.Id == activeTenant.Id);
        tenants.ShouldNotContain(t => t.Id == deletedTenant.Id);
    }

    [Fact]
    public async Task IgnoreQueryFilters_IncludesDeletedEntities()
    {
        var activeTenant = CreateTestTenant("Active Tenant");
        activeTenant.Id = Guid.NewGuid();
        var deletedTenant = CreateTestTenant("Deleted Tenant");
        deletedTenant.Id = Guid.NewGuid();

        // Mark as deleted using reflection since IsDeleted is private setter
        var isDeletedProperty = typeof(BaseEntity).GetProperty("IsDeleted");
        isDeletedProperty?.SetValue(deletedTenant, true);

        _context.Tenants.AddRange(activeTenant, deletedTenant);
        await _context.SaveChangesAsync();

        var tenants = await _context.Tenants.IgnoreQueryFilters().ToListAsync();

        tenants.Count.ShouldBe(2);
        tenants.ShouldContain(t => t.Id == activeTenant.Id);
        tenants.ShouldContain(t => t.Id == deletedTenant.Id);
    }

    [Fact]
    public async Task SaveChangesAsync_HandlesMultipleEntitiesCorrectly()
    {
        var tenant1 = CreateTestTenant("Tenant 1");
        var tenant2 = CreateTestTenant("Tenant 2");
        var tenant3 = CreateTestTenant("Tenant 3");
        tenant3.Id = Guid.NewGuid();

        _context.Tenants.AddRange(tenant1, tenant2, tenant3);
        await _context.SaveChangesAsync();

        tenant1.Id.ShouldNotBe(Guid.Empty);
        tenant2.Id.ShouldNotBe(Guid.Empty);
        tenant3.Id.ShouldNotBe(_generatedId);

        tenant1.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        tenant2.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        tenant3.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }
}
