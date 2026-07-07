namespace Workplan.Client.Models;

public record ProjectDto(Guid Id, string Code, string Name, Guid? PmUserId, bool IsActive);

public record CreateProjectRequest(string Code, string Name, Guid? PmUserId);

public record UpdateProjectRequest(string Code, string Name, Guid? PmUserId = null);
