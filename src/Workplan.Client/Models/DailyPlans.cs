namespace Workplan.Client.Models;

public record DailyPlanDto(
    Guid Id,
    Guid ProjectId,
    Guid CrewRegionId,
    Guid LocationId,
    Guid WorkItemTypeId,
    DateOnly WorkDate,
    Guid PlannedById,
    Guid? AssignedHoMId,
    Guid? CrewTypeId,
    decimal PlannedQuantity,
    decimal PlannedManDay,
    Unit Unit,
    decimal? FactQuantity,
    decimal? FactManDay,
    decimal? Overtime,
    string? Comment,
    WorkStatus Status,
    IReadOnlyCollection<StatusTransitionDto>? History = null);

public record StatusTransitionDto(
    WorkStatus FromStatus,
    WorkStatus ToStatus,
    Guid ActionById,
    DateTime TransitionedAt,
    string? ActionByName,
    string? Note);

public record DailyPlanListItemDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid CrewRegionId,
    string CrewRegionName,
    Guid LocationId,
    string LocationName,
    Guid WorkItemTypeId,
    string WorkItemTypeName,
    DateOnly WorkDate,
    Guid PlannedById,
    Guid? AssignedHoMId,
    Guid? CrewTypeId,
    string? CrewTypeName,
    decimal PlannedQuantity,
    decimal PlannedManDay,
    Unit Unit,
    decimal? FactQuantity,
    decimal? FactManDay,
    decimal? Overtime,
    string? Comment,
    WorkStatus Status,
    string? LatestRejectionReason = null,
    DateTime? LatestRejectionAtUtc = null,
    WorkStatus? LatestRejectionFromStatus = null);

public record DailyTrackingPlanDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid CrewRegionId,
    string CrewRegionName,
    Guid LocationId,
    string LocationName,
    Guid WorkItemTypeId,
    string WorkItemTypeName,
    DateOnly WorkDate,
    Guid PlannedById,
    Guid? AssignedHoMId,
    string? AssignedHoMName,
    string? SiteChiefName,
    string? ProjectManagerName,
    Guid? CrewTypeId,
    string? CrewTypeName,
    decimal PlannedQuantity,
    decimal PlannedManDay,
    Unit Unit,
    decimal? FactQuantity,
    decimal? FactManDay,
    decimal? Overtime,
    string? Comment,
    WorkStatus Status);

public record DailyTrackingOptionsDto(
    IReadOnlyList<TrackingFilterOptionDto> Projects,
    IReadOnlyList<TrackingFilterOptionDto> CrewRegions,
    IReadOnlyList<TrackingFilterOptionDto> Locations,
    IReadOnlyList<TrackingFilterOptionDto> HeadOfMasters,
    IReadOnlyList<TrackingFilterOptionDto> SiteChiefs,
    IReadOnlyList<TrackingFilterOptionDto> ProjectManagers,
    IReadOnlyList<WorkStatus> Statuses);

public record TrackingFilterOptionDto(Guid Id, string Label);

public record DailyPlanDetailDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid CrewRegionId,
    string CrewRegionName,
    Guid LocationId,
    string LocationName,
    Guid WorkItemTypeId,
    string WorkItemTypeName,
    DateOnly WorkDate,
    Guid PlannedById,
    Guid? AssignedHoMId,
    string? AssignedHoMName,
    string? SiteChiefName,
    string? ProjectManagerName,
    Guid? CrewTypeId,
    string? CrewTypeName,
    decimal PlannedQuantity,
    decimal PlannedManDay,
    Unit Unit,
    decimal? FactQuantity,
    decimal? FactManDay,
    decimal? Overtime,
    string? Comment,
    WorkStatus Status,
    IReadOnlyCollection<StatusTransitionDto> History,
    IReadOnlyCollection<DailyPlanCommentDto> Comments);

public record DailyPlanCommentDto(
    DateTime CreatedAt,
    Guid ActionById,
    string? ActionByName,
    WorkStatus FromStatus,
    WorkStatus ToStatus,
    DailyPlanCommentKind Kind,
    string Text);

public record CreateDailyPlanRequest(
    Guid ProjectId,
    Guid CrewRegionId,
    Guid LocationId,
    Guid WorkItemTypeId,
    DateOnly WorkDate,
    decimal PlannedQuantity,
    decimal PlannedManDay,
    Guid AssignedHoMId);

public record StartWorkRequest(Guid CrewTypeId);

public record SubmitProgressRequest(decimal? FactQuantity, decimal? FactManDay, decimal? Overtime, string? Comment);

public record RejectRequest(string Reason);
