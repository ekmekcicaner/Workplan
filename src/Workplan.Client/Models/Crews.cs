namespace Workplan.Client.Models;

public record CrewMemberDto(Guid Id, WorkerType WorkerType, string PersonnelRef);

public record CrewDto(Guid Id, Guid LocationId, string Name, Guid CreatedByHoMId, List<CrewMemberDto> Members);

public record CreateCrewRequest(Guid LocationId, string Name);

public record AddCrewMemberRequest(WorkerType WorkerType, string PersonnelRef);
