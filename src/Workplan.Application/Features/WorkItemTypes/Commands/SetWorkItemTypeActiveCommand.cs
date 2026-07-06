using Mediator;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.WorkItemTypes.Commands;

public sealed record SetWorkItemTypeActiveCommand(Guid Id, bool IsActive) : IRequest<Result>;
