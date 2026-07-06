using FluentValidation;
using RoleNames = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.Application.Features.Users.Commands;

public class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Roles)
            .NotEmpty()
            .WithMessage("En az bir rol atanmalı.")
            .Must(roles => roles.All(RoleNames.All.Contains))
            .WithMessage($"Roller şunlardan biri olmalı: {string.Join(", ", RoleNames.All)}");
    }
}
