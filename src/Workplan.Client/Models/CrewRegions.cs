namespace Workplan.Client.Models;

public record CrewRegionDto(
    Guid Id, Guid ProjectId, string Code, string Name, Guid? SiteChiefUserId, Guid? TechOfficeUserId, bool IsActive);

public record CreateCrewRegionRequest(Guid ProjectId, string Code, string Name);

public record UpdateCrewRegionRequest(string Code, string Name);

public record AssignSiteChiefRequest(Guid SiteChiefUserId);

public record AssignTechOfficeRequest(Guid TechOfficeUserId);
