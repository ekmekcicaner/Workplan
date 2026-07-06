using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Locations.Commands;

public sealed record AssignHeadOfMasterCommand(Guid LocationId, Guid HeadOfMasterUserId) : IRequest<Result>;
