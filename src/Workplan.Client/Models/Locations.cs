namespace Workplan.Client.Models;

public record LocationDto(
    Guid Id, Guid ProjectId, Guid CrewRegionId, string Name, Guid? ParentId, Guid? HeadOfMasterUserId, bool IsActive);

public record CreateLocationRequest(Guid ProjectId, Guid CrewRegionId, string Name, Guid? ParentId);

public record AssignHeadOfMasterRequest(Guid HeadOfMasterUserId);

public record UpdateLocationRequest(string Name);
