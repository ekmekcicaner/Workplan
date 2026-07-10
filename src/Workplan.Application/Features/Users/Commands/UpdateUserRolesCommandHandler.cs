using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Users.Commands;

public class UpdateUserRolesCommandHandler(IIdentityService identityService)
    : IRequestHandler<UpdateUserRolesCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        => await identityService.UpdateUserRolesAsync(request.UserId, request.Roles, cancellationToken);
}
