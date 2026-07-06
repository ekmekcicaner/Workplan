using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery(string? Role) : IRequest<Result<List<UserDto>>>;
