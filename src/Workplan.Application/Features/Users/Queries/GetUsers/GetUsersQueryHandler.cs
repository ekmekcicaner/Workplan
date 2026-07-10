using Mediator;
using Workplan.Application.Interfaces;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUsersQuery, Result<List<UserDto>>>
{
    public async ValueTask<Result<List<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await identityService.GetUsersAsync(request.Role, cancellationToken);
        return users.Select(u => new UserDto(u.Id, u.Email, u.FullName, u.Roles, u.IsActive)).ToList();
    }
}
