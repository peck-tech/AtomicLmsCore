using AtomicLmsCore.Application.Tenants.Queries;
using FluentValidation;
using JetBrains.Annotations;

namespace AtomicLmsCore.Application.Tenants.Validators;

/// <summary>
///     Validator for GetTenantByIdQuery.
/// </summary>
public class GetTenantByIdQueryValidator : AbstractValidator<GetTenantByIdQuery>
{
    public GetTenantByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");
    }
}
