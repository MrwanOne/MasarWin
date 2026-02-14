using FluentValidation;
using Masar.Application.DTOs;

namespace Masar.Application.Validators;

public class TeamValidator : AbstractValidator<TeamDto>
{
    public TeamValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الفريق مطلوب / Team name is required")
            .MinimumLength(2).WithMessage("الاسم قصير جداً / Name is too short")
            .MaximumLength(100).WithMessage("الاسم طويل جداً / Name is too long");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("يجب اختيار القسم / Department must be selected");
    }
}
