using FluentValidation;

namespace Workplan.Application.Features.WorkItemTypes.Commands;

public class CreateWorkItemTypeCommandValidator : AbstractValidator<CreateWorkItemTypeCommand>
{
    public CreateWorkItemTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
