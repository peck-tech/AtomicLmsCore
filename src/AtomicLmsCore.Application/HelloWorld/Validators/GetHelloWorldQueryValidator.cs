using AtomicLmsCore.Application.HelloWorld.Queries;
using FluentValidation;

namespace AtomicLmsCore.Application.HelloWorld.Validators;

public class GetHelloWorldQueryValidator : AbstractValidator<GetHelloWorldQuery>
{
    public GetHelloWorldQueryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");
    }
}