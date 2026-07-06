using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<Guid>>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async ValueTask<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        => await _identityService.RegisterAsync(
            request.Email, request.Password, request.FullName, request.Roles, cancellationToken);
}
