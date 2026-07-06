using Mediator;
using Workplan.SharedKernel.Common;
using WorkUnit = Workplan.Domain.ValueObjects.Unit;

namespace Workplan.Application.Features.WorkItemTypes.Commands;

public sealed record CreateWorkItemTypeCommand(string Name, Guid? ParentId, WorkUnit Unit = WorkUnit.None)
    : IRequest<Result<Guid>>;
