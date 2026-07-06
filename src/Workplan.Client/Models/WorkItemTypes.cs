namespace Workplan.Client.Models;

public enum WorkUnit
{
    None,
    Ton,
    M3,
    M2
}

public record WorkItemTypeDto(Guid Id, string Name, Guid? ParentId, int Level, bool IsActive, WorkUnit Unit)
{
    public List<WorkItemTypeDto> Children { get; init; } = [];
}

public record CreateWorkItemTypeRequest(string Name, Guid? ParentId, WorkUnit Unit = WorkUnit.None);

public record UpdateWorkItemTypeRequest(string Name, WorkUnit Unit = WorkUnit.None);
