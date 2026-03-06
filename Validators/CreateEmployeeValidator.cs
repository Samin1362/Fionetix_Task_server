using FluentValidation;
using FionetixAPI.DTOs;

namespace FionetixAPI.Validators;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.NID)
            .NotEmpty().WithMessage("NID is required.")
            .Matches(@"^(\d{10}|\d{17})$").WithMessage("NID must be exactly 10 or 17 digits.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Matches(@"^(\+8801[3-9]\d{8}|01[3-9]\d{8})$")
            .WithMessage("Phone must be a valid Bangladesh number (starting with +880 or 01).");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.")
            .MaximumLength(100).WithMessage("Department must not exceed 100 characters.");

        RuleFor(x => x.BasicSalary)
            .GreaterThan(0).WithMessage("Basic salary must be greater than 0.");

        When(x => x.Spouse != null, () =>
        {
            RuleFor(x => x.Spouse!.Name)
                .NotEmpty().WithMessage("Spouse name is required.");

            RuleFor(x => x.Spouse!.NID)
                .Matches(@"^(\d{10}|\d{17})$")
                .When(x => !string.IsNullOrEmpty(x.Spouse?.NID))
                .WithMessage("Spouse NID must be exactly 10 or 17 digits.");
        });

        When(x => x.Children != null && x.Children.Count > 0, () =>
        {
            RuleForEach(x => x.Children).ChildRules(child =>
            {
                child.RuleFor(c => c.Name)
                    .NotEmpty().WithMessage("Child name is required.");

                child.RuleFor(c => c.DateOfBirth)
                    .Must(dob => dob <= DateOnly.FromDateTime(DateTime.Today))
                    .WithMessage("Date of birth cannot be in the future.");
            });
        });
    }
}

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeDto>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.NID)
            .NotEmpty().WithMessage("NID is required.")
            .Matches(@"^(\d{10}|\d{17})$").WithMessage("NID must be exactly 10 or 17 digits.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Matches(@"^(\+8801[3-9]\d{8}|01[3-9]\d{8})$")
            .WithMessage("Phone must be a valid Bangladesh number (starting with +880 or 01).");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.")
            .MaximumLength(100).WithMessage("Department must not exceed 100 characters.");

        RuleFor(x => x.BasicSalary)
            .GreaterThan(0).WithMessage("Basic salary must be greater than 0.");
    }
}
