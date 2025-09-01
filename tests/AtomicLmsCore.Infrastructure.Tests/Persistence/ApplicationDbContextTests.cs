using AtomicLmsCore.Domain;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

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

        tenant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tenant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        (tenant.CreatedAt - tenant.UpdatedAt).Duration().Should().BeLessThan(TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task SaveChangesAsync_SetsId_ForNewEntitiesWithoutId()
    {
        var tenant = CreateTestTenant("New Tenant");
        tenant.Id.Should().Be(Guid.Empty);

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();

        tenant.Id.Should().Be(_generatedId);
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

        tenant.Id.Should().Be(existingId);
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

        tenant.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        tenant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tenant.CreatedAt.Should().Be(tenant.CreatedAt);
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

        tenant.CreatedAt.Should().Be(originalCreatedAt);
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
        tenants.Count.Should().Be(1);
        tenants.Should().Contain(t => t.Id == activeTenant.Id);
        tenants.Should().NotContain(t => t.Id == deletedTenant.Id);
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

        tenants.Count.Should().Be(2);
        tenants.Should().Contain(t => t.Id == activeTenant.Id);
        tenants.Should().Contain(t => t.Id == deletedTenant.Id);
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

        tenant1.Id.Should().NotBe(Guid.Empty);
        tenant2.Id.Should().NotBe(Guid.Empty);
        tenant3.Id.Should().NotBe(_generatedId);

        tenant1.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tenant2.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tenant3.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
