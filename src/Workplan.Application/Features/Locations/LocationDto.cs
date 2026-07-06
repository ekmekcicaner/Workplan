namespace Workplan.Application.Features.Locations;

public sealed record LocationDto(
    Guid Id,
    Guid ProjectId,
    Guid CrewRegionId,
    string Name,
    Guid? ParentId,
    Guid? HeadOfMasterUserId,
    bool IsActive);
