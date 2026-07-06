using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Users.Commands;

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ResetUserPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async ValueTask<Result> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
        => await _identityService.ResetPasswordAsync(request.UserId, request.NewPassword, cancellationToken);
}
