using FluentValidation;
using RoleNames = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.Application.Features.Auth.Commands;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FullName).NotEmpty();
        RuleFor(x => x.Roles)
            .NotEmpty()
            .WithMessage("En az bir rol atanmalı.")
            .Must(roles => roles.All(RoleNames.All.Contains))
            .WithMessage($"Roller şunlardan biri olmalı: {string.Join(", ", RoleNames.All)}");
    }
}
