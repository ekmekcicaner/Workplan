using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Auth.Commands;

public class RegisterCommandHandler(IIdentityService identityService) : IRequestHandler<RegisterCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        => await identityService.RegisterAsync(
            request.Email, request.Password, request.FullName, request.Roles, cancellationToken);
}
