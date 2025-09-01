using AtomicLmsCore.Application.Tenants.Commands;
using AtomicLmsCore.Application.Tenants.Validators;
using FluentValidation.TestHelper;

namespace AtomicLmsCore.Application.Tests.Tenants.Validators;

public class CreateTenantCommandValidatorTests
{
    private readonly CreateTenantCommandValidator _validator;

    public CreateTenantCommandValidatorTests()
    {
        _validator = new();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var command = new CreateTenantCommand("", "test-slug", "testdb");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Tenant name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        // Arrange
        var longName = new string('a', 256);
        var command = new CreateTenantCommand(longName, "test-slug", "testdb");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Tenant name must not exceed 255 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_Slug_Is_Empty()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Name", "", "testdb");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Slug)
            .WithErrorMessage("Tenant slug is required.");
    }

    [Fact]
    public void Should_Have_Error_When_Slug_Contains_Invalid_Characters()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Name", "Test_Slug!", "testdb");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Slug)
            .WithErrorMessage("Tenant slug must contain only lowercase letters, numbers, and hyphens.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Tenant", "test-tenant", "testdb");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Metadata_Has_Too_Many_Items()
    {
        // Arrange
        var metadata = new Dictionary<string, string>();
        for (var i = 0; i < 51; i++)
        {
            metadata[$"key{i}"] = $"value{i}";
        }

        var command = new CreateTenantCommand("Test Tenant", "test-tenant", "testdb", true, metadata);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Metadata)
            .WithErrorMessage("Metadata cannot contain more than 50 items.");
    }

    [Fact]
    public void Should_Have_Error_When_DatabaseName_Is_Empty()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Name", "test-slug", "");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DatabaseName)
            .WithErrorMessage("Database name is required.");
    }

    [Fact]
    public void Should_Have_Error_When_DatabaseName_Exceeds_MaxLength()
    {
        // Arrange
        var longDbName = new string('a', 256);
        var command = new CreateTenantCommand("Test Name", "test-slug", longDbName);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DatabaseName)
            .WithErrorMessage("Database name must not exceed 255 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_DatabaseName_Contains_Invalid_Characters()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Name", "test-slug", "test-db!");

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DatabaseName)
            .WithErrorMessage("Database name must contain only letters, numbers, and underscores.");
    }
}
