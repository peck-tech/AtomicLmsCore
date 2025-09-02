using AtomicLmsCore.Domain.Entities;
using AtomicLmsCore.Domain.Services;
using AtomicLmsCore.Infrastructure.Persistence;
using AtomicLmsCore.Infrastructure.Persistence.Repositories;
using AtomicLmsCore.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AtomicLmsCore.Infrastructure.Tests.Persistence.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly TenantDbContext _context;
    private readonly UserRepository _repository;
    private readonly IIdGenerator _idGenerator;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseInMemoryDatabase($"UserRepositoryTests_{Guid.NewGuid()}")
            .Options;

        _idGenerator = new UlidIdGenerator();
        _context = new TenantDbContext(options, _idGenerator);
        var loggerMock = new Mock<ILogger<UserRepository>>();
        _repository = new UserRepository(_context, loggerMock.Object);
    }

    public void Dispose()
        => _context.Dispose();

    public class GetAllAsyncTests : UserRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_WhenUsersExist_ReturnsUsers()
        {
            // Arrange
            var user = new User
            {
                Id = _idGenerator.NewId(),
                ExternalUserId = "external|test123",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                DisplayName = "John Doe",
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value.First().Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetAllAsync_WhenNoUsers_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }
    }

    public class GetByIdAsyncTests : UserRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
        {
            // Arrange
            var user = new User
            {
                Id = _idGenerator.NewId(),
                ExternalUserId = "external|test123",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                DisplayName = "John Doe",
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(user.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            // Arrange
            var nonExistentId = _idGenerator.NewId();

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }
    }

    public class GetByExternalUserIdAsyncTests : UserRepositoryTests
    {
        [Fact]
        public async Task GetByExternalUserIdAsync_WhenUserExists_ReturnsUser()
        {
            // Arrange
            var user = new User
            {
                Id = _idGenerator.NewId(),
                ExternalUserId = "external|test123",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                DisplayName = "John Doe",
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByExternalUserIdAsync("external|test123");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetByExternalUserIdAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByExternalUserIdAsync("external|nonexistent");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }
    }

    public class AddAsyncTests : UserRepositoryTests
    {
        [Fact]
        public async Task AddAsync_WhenValidUser_AddsUser()
        {
            // Arrange
            var user = new User
            {
                Id = _idGenerator.NewId(),
                ExternalUserId = "external|test123",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                DisplayName = "John Doe",
                IsActive = true
            };

            // Act
            var result = await _repository.AddAsync(user);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.InternalId.Should().BeGreaterThan(0);

            var savedUser = await _context.Users.FindAsync(result.Value.InternalId);
            savedUser.Should().NotBeNull();
            savedUser!.Email.Should().Be("test@example.com");
        }
    }

    public class EmailExistsAsyncTests : UserRepositoryTests
    {
        [Fact]
        public async Task EmailExistsAsync_WhenEmailExists_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Id = _idGenerator.NewId(),
                ExternalUserId = "external|test123",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                DisplayName = "John Doe",
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.EmailExistsAsync("test@example.com");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public async Task EmailExistsAsync_WhenEmailDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _repository.EmailExistsAsync("nonexistent@example.com");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Fact]
        public async Task EmailExistsAsync_WithExcludeUserId_ExcludesSpecifiedUser()
        {
            // Arrange
            var user = new User
            {
                Id = _idGenerator.NewId(),
                ExternalUserId = "external|test123",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                DisplayName = "John Doe",
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.EmailExistsAsync("test@example.com", user.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }
    }
}
