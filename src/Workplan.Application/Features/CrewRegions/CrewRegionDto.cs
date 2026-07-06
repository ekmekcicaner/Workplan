namespace Workplan.Application.Features.CrewRegions;

public sealed record CrewRegionDto(
    Guid Id,
    Guid ProjectId,
    string Code,
    string Name,
    Guid? SiteChiefUserId,
    Guid? TechOfficeUserId,
    bool IsActive);
