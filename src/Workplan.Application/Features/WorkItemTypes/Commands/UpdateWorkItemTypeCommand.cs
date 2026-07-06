using Mediator;
using Workplan.SharedKernel.Common;
using WorkUnit = Workplan.Domain.ValueObjects.Unit;

namespace Workplan.Application.Features.WorkItemTypes.Commands;

public sealed record UpdateWorkItemTypeCommand(Guid Id, string Name, WorkUnit Unit = WorkUnit.None) : IRequest<Result>;
