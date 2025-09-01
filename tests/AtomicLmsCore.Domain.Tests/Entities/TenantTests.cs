using AtomicLmsCore.Domain.Entities;
using FluentAssertions;

namespace AtomicLmsCore.Domain.Tests.Entities;

public class TenantTests
{
    [Fact]
    public void Tenant_Inherits_From_BaseEntity()
    {
        var tenant = new Tenant();

        tenant.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Tenant_Name_Defaults_To_Empty_String()
    {
        var tenant = new Tenant();

        tenant.Name.Should().Be(string.Empty);
    }

    [Fact]
    public void Tenant_Name_Can_Be_Set()
    {
        var tenant = new Tenant { Name = "Test Tenant" };

        tenant.Name.Should().Be("Test Tenant");
    }

    [Fact]
    public void Tenant_Slug_Defaults_To_Empty_String()
    {
        var tenant = new Tenant();

        tenant.Slug.Should().Be(string.Empty);
    }

    [Fact]
    public void Tenant_Slug_Can_Be_Set()
    {
        var tenant = new Tenant { Slug = "test-tenant" };

        tenant.Slug.Should().Be("test-tenant");
    }

    [Fact]
    public void Tenant_IsActive_Defaults_To_True()
    {
        var tenant = new Tenant();

        tenant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Tenant_IsActive_Can_Be_Set()
    {
        var tenant = new Tenant { IsActive = false };

        tenant.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Tenant_Metadata_Defaults_To_Empty_Dictionary()
    {
        var tenant = new Tenant();

        tenant.Metadata.Should().NotBeNull();
        tenant.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Tenant_Metadata_Can_Be_Set()
    {
        var metadata = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

        var tenant = new Tenant { Metadata = metadata };

        tenant.Metadata.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public void Tenant_Has_BaseEntity_Properties()
    {
        var tenant = new Tenant();

        tenant.Id.Should().Be(Guid.Empty);
        tenant.InternalId.Should().Be(0);
        tenant.CreatedAt.Should().Be(default);
        tenant.UpdatedAt.Should().Be(default);
        tenant.CreatedBy.Should().Be(string.Empty);
        tenant.UpdatedBy.Should().Be(string.Empty);
        tenant.IsDeleted.Should().BeFalse();
    }
}
