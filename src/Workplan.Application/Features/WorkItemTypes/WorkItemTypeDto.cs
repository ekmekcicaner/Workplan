using WorkUnit = Workplan.Domain.ValueObjects.Unit;

namespace Workplan.Application.Features.WorkItemTypes;

public sealed record WorkItemTypeDto(Guid Id, string Name, Guid? ParentId, int Level, bool IsActive, WorkUnit Unit)
{
    public List<WorkItemTypeDto> Children { get; init; } = [];
}
