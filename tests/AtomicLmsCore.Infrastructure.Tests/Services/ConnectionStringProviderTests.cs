using AtomicLmsCore.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AtomicLmsCore.Infrastructure.Tests.Services;

public class ConnectionStringProviderTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ConnectionStringProvider _provider;

    public ConnectionStringProviderTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _provider = new ConnectionStringProvider(_mockConfiguration.Object);
    }

    [Fact]
    public void GetSolutionsConnectionString_ValidConnectionString_ReturnsConnectionString()
    {
        // Arrange
        var expectedConnectionString = "Server=localhost;Database=Solutions;";
        var mockConnectionStringsSection = new Mock<IConfigurationSection>();

        mockConnectionStringsSection.Setup(x => x["SolutionsDatabase"]).Returns(expectedConnectionString);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act
        var result = _provider.GetSolutionsConnectionString();

        // Assert
        result.Should().Be(expectedConnectionString);
    }

    [Fact]
    public void GetSolutionsConnectionString_MissingConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConnectionStringsSection = new Mock<IConfigurationSection>();
        mockConnectionStringsSection.Setup(x => x["SolutionsDatabase"]).Returns((string?)null);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act & Assert
        var action = () => _provider.GetSolutionsConnectionString();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Solutions database connection string not configured");
    }

    [Fact]
    public void GetSolutionsConnectionString_EmptyConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConnectionStringsSection = new Mock<IConfigurationSection>();
        mockConnectionStringsSection.Setup(x => x["SolutionsDatabase"]).Returns(string.Empty);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act & Assert
        var action = () => _provider.GetSolutionsConnectionString();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Solutions database connection string not configured");
    }

    [Fact]
    public void GetTenantConnectionString_ValidTemplate_ReturnsFormattedConnectionString()
    {
        // Arrange
        var template = "Server=localhost;Database={DatabaseName};";
        var databaseName = "TenantDb123";
        var expectedResult = "Server=localhost;Database=TenantDb123;";

        var mockConnectionStringsSection = new Mock<IConfigurationSection>();
        mockConnectionStringsSection.Setup(x => x["TenantDatabaseTemplate"]).Returns(template);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act
        var result = _provider.GetTenantConnectionString(databaseName);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void GetTenantConnectionString_TemplateWithMultiplePlaceholders_ReplacesAll()
    {
        // Arrange
        var template = "Server=localhost;Database={DatabaseName};Initial Catalog={DatabaseName};";
        var databaseName = "TenantDb123";
        var expectedResult = "Server=localhost;Database=TenantDb123;Initial Catalog=TenantDb123;";

        var mockConnectionStringsSection = new Mock<IConfigurationSection>();
        mockConnectionStringsSection.Setup(x => x["TenantDatabaseTemplate"]).Returns(template);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act
        var result = _provider.GetTenantConnectionString(databaseName);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void GetTenantConnectionString_MissingTemplate_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConnectionStringsSection = new Mock<IConfigurationSection>();
        mockConnectionStringsSection.Setup(x => x["TenantDatabaseTemplate"]).Returns((string?)null);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act & Assert
        var action = () => _provider.GetTenantConnectionString("TestDb");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Tenant database connection string template not configured");
    }

    [Fact]
    public void GetTenantConnectionString_EmptyTemplate_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConnectionStringsSection = new Mock<IConfigurationSection>();
        mockConnectionStringsSection.Setup(x => x["TenantDatabaseTemplate"]).Returns(string.Empty);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act & Assert
        var action = () => _provider.GetTenantConnectionString("TestDb");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Tenant database connection string template not configured");
    }

    [Fact]
    public async Task TenantDatabaseExistsAsync_NoMasterConnectionString_ReturnsTrue()
    {
        // Arrange
        var mockConnectionStringsSection = new Mock<IConfigurationSection>();
        mockConnectionStringsSection.Setup(x => x["MasterDatabase"]).Returns((string?)null);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act
        var result = await _provider.TenantDatabaseExistsAsync("TestDb");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TenantDatabaseExistsAsync_EmptyMasterConnectionString_ReturnsTrue()
    {
        // Arrange
        var mockConnectionStringsSection = new Mock<IConfigurationSection>();
        mockConnectionStringsSection.Setup(x => x["MasterDatabase"]).Returns(string.Empty);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act
        var result = await _provider.TenantDatabaseExistsAsync("TestDb");

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("TestDatabase")]
    [InlineData("MyTenant")]
    [InlineData("AnotherDb123")]
    public void GetTenantConnectionString_DifferentDatabaseNames_ReplacesCorrectly(string databaseName)
    {
        // Arrange
        var template = "Server=localhost;Database={DatabaseName};Trusted_Connection=true;";
        var expectedResult = $"Server=localhost;Database={databaseName};Trusted_Connection=true;";

        var mockConnectionStringsSection = new Mock<IConfigurationSection>();
        mockConnectionStringsSection.Setup(x => x["TenantDatabaseTemplate"]).Returns(template);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act
        var result = _provider.GetTenantConnectionString(databaseName);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void GetTenantConnectionString_SpecialCharactersInDatabaseName_HandlesCorrectly()
    {
        // Arrange
        var template = "Server=localhost;Database={DatabaseName};";
        var databaseName = "Tenant_Db-123.Test";
        var expectedResult = "Server=localhost;Database=Tenant_Db-123.Test;";

        var mockConnectionStringsSection = new Mock<IConfigurationSection>();
        mockConnectionStringsSection.Setup(x => x["TenantDatabaseTemplate"]).Returns(template);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act
        var result = _provider.GetTenantConnectionString(databaseName);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void GetTenantConnectionString_CaseSensitivePlaceholder_ReplacesExactMatch()
    {
        // Arrange
        var template = "Server=localhost;Database={DatabaseName};Catalog={databasename};";
        var databaseName = "TestDb";
        var expectedResult = "Server=localhost;Database=TestDb;Catalog={databasename};"; // Only exact case match replaced

        var mockConnectionStringsSection = new Mock<IConfigurationSection>();
        mockConnectionStringsSection.Setup(x => x["TenantDatabaseTemplate"]).Returns(template);
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
            .Returns(mockConnectionStringsSection.Object);

        // Act
        var result = _provider.GetTenantConnectionString(databaseName);

        // Assert
        result.Should().Be(expectedResult);
    }
}
