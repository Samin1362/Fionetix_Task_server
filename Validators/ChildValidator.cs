using FluentValidation;
using FionetixAPI.DTOs;

namespace FionetixAPI.Validators;

public class CreateChildValidator : AbstractValidator<CreateChildDto>
{
    public CreateChildValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Child name is required.");

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Date of birth cannot be in the future.");
    }
}

public class UpdateChildValidator : AbstractValidator<UpdateChildDto>
{
    public UpdateChildValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Child name is required.");

        RuleFor(x => x.DateOfBirth)
            .Must(dob => dob <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Date of birth cannot be in the future.");
    }
}
