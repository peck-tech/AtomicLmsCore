using AtomicLmsCore.Domain.Entities;
using Shouldly;

namespace AtomicLmsCore.Domain.Tests.Entities;

public class TenantTests
{
    [Fact]
    public void Tenant_Inherits_From_BaseEntity()
    {
        var tenant = new Tenant();

        tenant.ShouldBeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Tenant_Name_Defaults_To_Empty_String()
    {
        var tenant = new Tenant();

        tenant.Name.ShouldBe(string.Empty);
    }

    [Fact]
    public void Tenant_Name_Can_Be_Set()
    {
        var tenant = new Tenant { Name = "Test Tenant" };

        tenant.Name.ShouldBe("Test Tenant");
    }

    [Fact]
    public void Tenant_Slug_Defaults_To_Empty_String()
    {
        var tenant = new Tenant();

        tenant.Slug.ShouldBe(string.Empty);
    }

    [Fact]
    public void Tenant_Slug_Can_Be_Set()
    {
        var tenant = new Tenant { Slug = "test-tenant" };

        tenant.Slug.ShouldBe("test-tenant");
    }

    [Fact]
    public void Tenant_IsActive_Defaults_To_True()
    {
        var tenant = new Tenant();

        tenant.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Tenant_IsActive_Can_Be_Set()
    {
        var tenant = new Tenant { IsActive = false };

        tenant.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Tenant_Metadata_Defaults_To_Empty_Dictionary()
    {
        var tenant = new Tenant();

        tenant.Metadata.ShouldNotBeNull();
        tenant.Metadata.ShouldBeEmpty();
    }

    [Fact]
    public void Tenant_Metadata_Can_Be_Set()
    {
        var metadata = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

        var tenant = new Tenant { Metadata = metadata };

        tenant.Metadata.ShouldBeEquivalentTo(metadata);
    }

    [Fact]
    public void Tenant_Has_BaseEntity_Properties()
    {
        var tenant = new Tenant();

        tenant.Id.ShouldBe(Guid.Empty);
        tenant.InternalId.ShouldBe(0);
        tenant.CreatedAt.ShouldBe(default);
        tenant.UpdatedAt.ShouldBe(default);
        tenant.CreatedBy.ShouldBe(string.Empty);
        tenant.UpdatedBy.ShouldBe(string.Empty);
        tenant.IsDeleted.ShouldBeFalse();
    }
}
