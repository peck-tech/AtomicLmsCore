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
        var tenant = new Tenant
        {
            Name = "Test Tenant"
        };

        tenant.Name.Should().Be("Test Tenant");
    }

    [Fact]
    public void Tenant_Has_BaseEntity_Properties()
    {
        var tenant = new Tenant();

        tenant.Id.Should().BeEmpty();
        tenant.InternalId.Should().Be(0);
        tenant.CreatedAt.Should().Be(default);
        tenant.UpdatedAt.Should().Be(default);
        tenant.CreatedBy.Should().Be(string.Empty);
        tenant.UpdatedBy.Should().Be(string.Empty);
        tenant.IsDeleted.Should().BeFalse();
    }
}
