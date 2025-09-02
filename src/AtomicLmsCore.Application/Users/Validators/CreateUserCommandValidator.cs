using AtomicLmsCore.Application.Users.Commands;
using FluentValidation;
using JetBrains.Annotations;

namespace AtomicLmsCore.Application.Users.Validators;

/// <summary>
///     Validator for CreateUserCommand.
/// </summary>
[UsedImplicitly]
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.ExternalUserId)
            .NotEmpty().WithMessage("External User ID is required")
            .MaximumLength(255).WithMessage("External User ID must not exceed 255 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(200).WithMessage("Display name must not exceed 200 characters");
    }
}
