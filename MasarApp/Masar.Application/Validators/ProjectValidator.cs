using FluentValidation;
using Masar.Application.DTOs;

namespace Masar.Application.Validators;

public class ProjectValidator : AbstractValidator<ProjectDto>
{
    public ProjectValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان المشروع مطلوب / Project title is required")
            .MinimumLength(3).WithMessage("العنوان قصير جداً / Title is too short")
            .MaximumLength(300).WithMessage("العنوان طويل جداً / Title is too long");

        RuleFor(x => x.Beneficiary)
            .NotEmpty().WithMessage("الجهة المستفيدة مطلوبة / Beneficiary is required")
            .MaximumLength(200).WithMessage("اسم الجهة طويل جداً / Beneficiary name is too long");

        RuleFor(x => x.CompletionRate)
            .InclusiveBetween(0, 100).WithMessage("نسبة الإنجاز يجب أن تكون بين 0 و 100 / Completion rate must be between 0 and 100");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("يجب اختيار القسم / Department must be selected");

        RuleFor(x => x.CollegeId)
            .GreaterThan(0).WithMessage("يجب اختيار الكلية / College must be selected");
            
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("الوصف طويل جداً / Description is too long");
    }
}
