using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Users.Commands;

public class SetUserActiveCommandHandler(IIdentityService identityService)
    : IRequestHandler<SetUserActiveCommand, Result>
{
    public async ValueTask<Result> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
        => await identityService.SetUserActiveAsync(request.UserId, request.IsActive, cancellationToken);
}
