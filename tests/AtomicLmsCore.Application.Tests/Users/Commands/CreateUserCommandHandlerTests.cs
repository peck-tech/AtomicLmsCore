using AtomicLmsCore.Application.Common.Interfaces;
using AtomicLmsCore.Application.Users.Commands;
using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Application.Tests.Users.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IIdGenerator> _idGeneratorMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        var identityManagementServiceMock = new Mock<IIdentityManagementService>();
        _idGeneratorMock = new Mock<IIdGenerator>();
        var loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();

        _handler = new CreateUserCommandHandler(
            _userRepositoryMock.Object,
            identityManagementServiceMock.Object,
            _idGeneratorMock.Object,
            loggerMock.Object);
    }

    public class HandleTests : CreateUserCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenValidCommand_CreatesUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateUserCommand(
                "auth0|test123",
                "test@example.com",
                "John",
                "Doe",
                "John Doe",
                true,
                new Dictionary<string, string>());

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("test@example.com", null))
                .ReturnsAsync(Result.Ok(false));

            _userRepositoryMock
                .Setup(x => x.ExternalUserIdExistsAsync("auth0|test123"))
                .ReturnsAsync(Result.Ok(false));

            _idGeneratorMock
                .Setup(x => x.NewId())
                .Returns(userId);

            _userRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) =>
                {
                    user.InternalId = 1;
                    return Result.Ok(user);
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(userId);

            _userRepositoryMock.Verify(
                x => x.AddAsync(It.Is<User>(u =>
                    u.ExternalUserId == "auth0|test123" &&
                    u.Email == "test@example.com" &&
                    u.FirstName == "John" &&
                    u.LastName == "Doe" &&
                    u.DisplayName == "John Doe" &&
                    u.IsActive == true)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenEmailExists_ReturnsFailure()
        {
            // Arrange
            var command = new CreateUserCommand(
                "auth0|test123",
                "test@example.com",
                "John",
                "Doe",
                "John Doe",
                true,
                null);

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("test@example.com", null))
                .ReturnsAsync(Result.Ok(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "A user with this email already exists in the tenant");
        }

        [Fact]
        public async Task Handle_WhenExternalUserIdExists_ReturnsFailure()
        {
            // Arrange
            var command = new CreateUserCommand(
                "auth0|test123",
                "test@example.com",
                "John",
                "Doe",
                "John Doe",
                true,
                null);

            _userRepositoryMock
                .Setup(x => x.EmailExistsAsync("test@example.com", null))
                .ReturnsAsync(Result.Ok(false));

            _userRepositoryMock
                .Setup(x => x.ExternalUserIdExistsAsync("auth0|test123"))
                .ReturnsAsync(Result.Ok(true));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message == "A user with this external user ID already exists");
        }
    }
}
