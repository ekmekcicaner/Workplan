using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Users.Commands;

public class ResetUserPasswordCommandHandler(IIdentityService identityService)
    : IRequestHandler<ResetUserPasswordCommand, Result>
{
    public async ValueTask<Result> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
        => await identityService.ResetPasswordAsync(request.UserId, request.NewPassword, cancellationToken);
}
