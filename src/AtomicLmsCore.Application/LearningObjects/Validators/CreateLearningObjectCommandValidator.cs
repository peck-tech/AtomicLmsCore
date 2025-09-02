using AtomicLmsCore.Application.LearningObjects.Commands;
using FluentValidation;
using JetBrains.Annotations;

namespace AtomicLmsCore.Application.LearningObjects.Validators;

/// <summary>
///     Validator for CreateLearningObjectCommand.
/// </summary>
[UsedImplicitly]
public class CreateLearningObjectCommandValidator : AbstractValidator<CreateLearningObjectCommand>
{
    public CreateLearningObjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Learning object name is required.")
            .MaximumLength(500)
            .WithMessage("Learning object name cannot exceed 500 characters.");

        RuleFor(x => x.Metadata)
            .Must(metadata => metadata == null || metadata.Keys.All(key => !string.IsNullOrWhiteSpace(key)))
            .WithMessage("Metadata keys cannot be empty or whitespace.")
            .Must(metadata => metadata == null || metadata.Values.All(value => value != null))
            .WithMessage("Metadata values cannot be null.");
    }
}
