using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Tenants.Commands;
using AtomicLmsCore.Domain.Entities;
using Moq;
using Shouldly;

namespace AtomicLmsCore.Application.Tests.Tenants.Commands;

public class UpdateTenantCommandHandlerTests
{
    private readonly UpdateTenantCommandHandler _handler;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;

    public UpdateTenantCommandHandlerTests()
    {
        _tenantRepositoryMock = new();
        _handler = new(_tenantRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Tenant_Successfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var existingTenant = new Tenant { Id = tenantId, Name = "Old Name", Slug = "old-slug", IsActive = false };

        var command = new UpdateTenantCommand(tenantId, "New Name", "new-slug", true);

        _tenantRepositoryMock.Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTenant);
        _tenantRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant tenant, CancellationToken _) => tenant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        existingTenant.Name.ShouldBe("New Name");
        existingTenant.Slug.ShouldBe("new-slug");
        existingTenant.IsActive.ShouldBeTrue();

        _tenantRepositoryMock.Verify(x => x.UpdateAsync(existingTenant, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Tenant_Not_Found()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new UpdateTenantCommand(tenantId, "New Name", "new-slug", true);

        _tenantRepositoryMock.Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message.Contains("Tenant not found"));

        _tenantRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Repository_Throws_Exception()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new UpdateTenantCommand(tenantId, "New Name", "new-slug", true);

        _tenantRepositoryMock.Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message.Contains("Failed to update tenant"));
    }
}
