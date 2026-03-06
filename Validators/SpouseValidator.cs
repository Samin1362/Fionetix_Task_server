using FluentValidation;
using FionetixAPI.DTOs;

namespace FionetixAPI.Validators;

public class UpsertSpouseValidator : AbstractValidator<UpsertSpouseDto>
{
    public UpsertSpouseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Spouse name is required.");

        RuleFor(x => x.NID)
            .Matches(@"^(\d{10}|\d{17})$")
            .When(x => !string.IsNullOrEmpty(x.NID))
            .WithMessage("Spouse NID must be exactly 10 or 17 digits.");
    }
}
