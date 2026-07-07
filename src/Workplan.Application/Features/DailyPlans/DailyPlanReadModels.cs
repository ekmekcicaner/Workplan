using Workplan.Domain.Enums;
using Workplan.Domain.ValueObjects;

namespace Workplan.Application.Features.DailyPlans;

public sealed record DailyPlanListItemDto(
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

public sealed record DailyTrackingPlanDto(
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

public sealed record DailyTrackingOptionsDto(
    IReadOnlyList<TrackingFilterOptionDto> Projects,
    IReadOnlyList<TrackingFilterOptionDto> CrewRegions,
    IReadOnlyList<TrackingFilterOptionDto> Locations,
    IReadOnlyList<TrackingFilterOptionDto> HeadOfMasters,
    IReadOnlyList<TrackingFilterOptionDto> SiteChiefs,
    IReadOnlyList<TrackingFilterOptionDto> ProjectManagers,
    IReadOnlyList<WorkStatus> Statuses);

public sealed record TrackingFilterOptionDto(Guid Id, string Label);

public sealed record DailyPlanDetailDto(
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

public sealed record DailyPlanCommentDto(
    DateTime CreatedAt,
    Guid ActionById,
    string? ActionByName,
    WorkStatus FromStatus,
    WorkStatus ToStatus,
    DailyPlanCommentKind Kind,
    string Text);

public enum DailyPlanCommentKind
{
    Progress,
    Rejection
}
