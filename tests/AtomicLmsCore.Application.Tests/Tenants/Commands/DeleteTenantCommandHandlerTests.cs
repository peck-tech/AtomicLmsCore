using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Tenants.Commands;
using AtomicLmsCore.Domain.Entities;
using FluentAssertions;
using Moq;

namespace AtomicLmsCore.Application.Tests.Tenants.Commands;

public class DeleteTenantCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _mockRepository;
    private readonly DeleteTenantCommandHandler _handler;

    public DeleteTenantCommandHandlerTests()
    {
        _mockRepository = new Mock<ITenantRepository>();
        _handler = new DeleteTenantCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteTenantCommand(id);
        var tenant = new Tenant { Id = id, Name = "Test Tenant" };

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _mockRepository.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TenantNotFound_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteTenantCommand(id);

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Tenant not found");
        _mockRepository.Verify(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GetByIdThrowsException_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteTenantCommand(id);

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Failed to delete tenant: Database error");
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeleteThrowsException_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteTenantCommand(id);
        var tenant = new Tenant { Id = id, Name = "Test Tenant" };

        _mockRepository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _mockRepository.Setup(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Delete operation failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Failed to delete tenant: Delete operation failed");
    }

    [Fact]
    public async Task Handle_CancellationRequested_PassesCancellationToken()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new DeleteTenantCommand(id);
        var tenant = new Tenant { Id = id, Name = "Test Tenant" };
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockRepository.Setup(x => x.GetByIdAsync(id, cancellationToken))
            .ReturnsAsync(tenant);
        _mockRepository.Setup(x => x.DeleteAsync(id, cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepository.Verify(x => x.GetByIdAsync(id, cancellationToken), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(id, cancellationToken), Times.Once);
    }
}
