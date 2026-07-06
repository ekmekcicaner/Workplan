using FluentValidation;

namespace Workplan.Application.Features.WorkItemTypes.Commands;

public class UpdateWorkItemTypeCommandValidator : AbstractValidator<UpdateWorkItemTypeCommand>
{
    public UpdateWorkItemTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
