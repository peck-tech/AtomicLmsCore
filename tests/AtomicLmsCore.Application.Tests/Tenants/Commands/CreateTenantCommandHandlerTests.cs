using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Tenants.Commands;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using Moq;
using Shouldly;

namespace AtomicLmsCore.Application.Tests.Tenants.Commands;

public class CreateTenantCommandHandlerTests
{
    private readonly CreateTenantCommandHandler _handler;
    private readonly Mock<IIdGenerator> _idGeneratorMock;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;

    public CreateTenantCommandHandlerTests()
    {
        _tenantRepositoryMock = new();
        _idGeneratorMock = new();
        _handler = new(_tenantRepositoryMock.Object, _idGeneratorMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Tenant_Successfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateTenantCommand("Test Tenant", "test-tenant");
        var expectedTenant = new Tenant { Id = tenantId, Name = "Test Tenant", Slug = "test-tenant", IsActive = true };

        _idGeneratorMock.Setup(x => x.NewId()).Returns(tenantId);
        _tenantRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant tenant, CancellationToken _) => tenant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(tenantId);

        _tenantRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Tenant>(t =>
                t.Id == tenantId &&
                t.Name == "Test Tenant" &&
                t.Slug == "test-tenant" &&
                t.IsActive == true &&
                t.Metadata.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Create_Tenant_With_Metadata()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var metadata = new Dictionary<string, string> { { "key", "value" } };
        var command = new CreateTenantCommand("Test Tenant", "test-tenant", true, metadata);

        _idGeneratorMock.Setup(x => x.NewId()).Returns(tenantId);
        _tenantRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant tenant, CancellationToken _) => tenant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(tenantId);

        _tenantRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Tenant>(t =>
                t.Metadata.Count == 1 &&
                t.Metadata["key"] == "value"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Repository_Throws_Exception()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Tenant", "test-tenant");

        _idGeneratorMock.Setup(x => x.NewId()).Returns(Guid.NewGuid());
        _tenantRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Message.Contains("Failed to create tenant"));
    }
}
