using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Tenants.Queries;
using AtomicLmsCore.Domain.Entities;
using FluentAssertions;
using Moq;

namespace AtomicLmsCore.Application.Tests.Tenants.Queries;

public class GetTenantByIdQueryHandlerTests
{
    private readonly GetTenantByIdQueryHandler _handler;
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;

    public GetTenantByIdQueryHandlerTests()
    {
        _tenantRepositoryMock = new();
        _handler = new(_tenantRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Tenant_When_Found()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant { Id = tenantId, Name = "Test Tenant", Slug = "test-tenant", IsActive = true };

        var query = new GetTenantByIdQuery(tenantId);
        _tenantRepositoryMock.Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(tenant);
        result.Value.Id.Should().Be(tenantId);
        result.Value.Name.Should().Be("Test Tenant");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Tenant_Not_Found()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetTenantByIdQuery(tenantId);

        _tenantRepositoryMock.Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("Tenant not found"));
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Repository_Throws_Exception()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetTenantByIdQuery(tenantId);

        _tenantRepositoryMock.Setup(x => x.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Message.Contains("Failed to retrieve tenant"));
    }
}
