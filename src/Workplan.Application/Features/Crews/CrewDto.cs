using Workplan.Domain.Enums;

namespace Workplan.Application.Features.Crews;

public sealed record CrewMemberDto(Guid Id, WorkerType WorkerType, string PersonnelRef);

public sealed record CrewDto(Guid Id, Guid LocationId, string Name, Guid CreatedByHoMId, List<CrewMemberDto> Members);
