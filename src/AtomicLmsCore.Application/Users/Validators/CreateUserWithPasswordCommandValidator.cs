using AtomicLmsCore.Application.Users.Commands;
using FluentValidation;
using JetBrains.Annotations;

namespace AtomicLmsCore.Application.Users.Validators;

/// <summary>
///     Validator for CreateUserWithPasswordCommand.
/// </summary>
[UsedImplicitly]
public class CreateUserWithPasswordCommandValidator : AbstractValidator<CreateUserWithPasswordCommand>
{
    public CreateUserWithPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(320).WithMessage("Email cannot exceed 320 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(200).WithMessage("Display name cannot exceed 200 characters");
    }
}
