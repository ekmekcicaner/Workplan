namespace Workplan.Client.Models;

public record CrewTypeDto(Guid Id, string Name, bool IsActive);

public record CreateCrewTypeRequest(string Name);

public record UpdateCrewTypeRequest(string Name);
