using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Users.Commands;

public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, Result>
{
    private readonly IIdentityService _identityService;

    public UpdateUserRolesCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async ValueTask<Result> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        => await _identityService.UpdateUserRolesAsync(request.UserId, request.Roles, cancellationToken);
}
