namespace Workplan.Application.Features.Projects;

public sealed record ProjectDto(Guid Id, string Code, string Name, Guid? PmUserId, bool IsActive);
