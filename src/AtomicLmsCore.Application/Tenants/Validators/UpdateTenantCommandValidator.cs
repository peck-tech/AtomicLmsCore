using AtomicLmsCore.Application.Tenants.Commands;
using FluentValidation;
using JetBrains.Annotations;

namespace AtomicLmsCore.Application.Tenants.Validators;

/// <summary>
///     Validator for UpdateTenantCommand.
/// </summary>
[UsedImplicitly]
public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tenant name is required.")
            .MaximumLength(255)
            .WithMessage("Tenant name must not exceed 255 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Tenant slug is required.")
            .MaximumLength(100)
            .WithMessage("Tenant slug must not exceed 100 characters.")
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Tenant slug must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(x => x.Metadata)
            .Must(metadata => metadata == null || metadata.Count <= 50)
            .WithMessage("Metadata cannot contain more than 50 items.");

        RuleForEach(x => x.Metadata)
            .Must(kvp => kvp.Key.Length <= 100)
            .WithMessage("Metadata keys must not exceed 100 characters.")
            .When(x => x.Metadata != null);

        RuleForEach(x => x.Metadata)
            .Must(kvp => kvp.Value.Length <= 1000)
            .WithMessage("Metadata values must not exceed 1000 characters.")
            .When(x => x.Metadata != null);
    }
}
