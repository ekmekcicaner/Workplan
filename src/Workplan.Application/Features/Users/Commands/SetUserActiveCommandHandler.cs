using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Users.Commands;

public class SetUserActiveCommandHandler : IRequestHandler<SetUserActiveCommand, Result>
{
    private readonly IIdentityService _identityService;

    public SetUserActiveCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async ValueTask<Result> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
        => await _identityService.SetUserActiveAsync(request.UserId, request.IsActive, cancellationToken);
}
