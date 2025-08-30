using AtomicLmsCore.Application.HelloWorld.Queries;
using AtomicLmsCore.Application.HelloWorld.Validators;
using FluentValidation.TestHelper;

namespace AtomicLmsCore.Application.Tests.HelloWorld.Validators;

public class GetHelloWorldQueryValidatorTests
{
    private readonly GetHelloWorldQueryValidator _validator;

    public GetHelloWorldQueryValidatorTests()
    {
        _validator = new();
    }

    [Fact]
    public void Validate_WithValidName_ShouldNotHaveValidationError()
    {
        var query = new GetHelloWorldQuery { Name = "John Doe" };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullName_ShouldNotHaveValidationError()
    {
        var query = new GetHelloWorldQuery { Name = null };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldNotHaveValidationError()
    {
        var query = new GetHelloWorldQuery { Name = "" };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithExactly100Characters_ShouldNotHaveValidationError()
    {
        var query = new GetHelloWorldQuery { Name = new('a', 100) };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMoreThan100Characters_ShouldHaveValidationError()
    {
        var query = new GetHelloWorldQuery { Name = new('a', 101) };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_With150Characters_ShouldHaveValidationError()
    {
        var query = new GetHelloWorldQuery { Name = new('x', 150) };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_WithSpecialCharacters_ShouldNotHaveValidationError()
    {
        var query = new GetHelloWorldQuery { Name = "John@123!#$%^&*()" };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithUnicodeCharacters_ShouldNotHaveValidationError()
    {
        var query = new GetHelloWorldQuery { Name = "José García 日本語" };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
