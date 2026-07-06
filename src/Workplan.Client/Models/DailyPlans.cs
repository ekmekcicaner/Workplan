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
    Guid? CrewId,
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
    string? Note);

public record CreateDailyPlanRequest(
    Guid ProjectId,
    Guid CrewRegionId,
    Guid LocationId,
    Guid WorkItemTypeId,
    DateOnly WorkDate,
    decimal PlannedQuantity,
    decimal PlannedManDay,
    Guid AssignedHoMId);

public record StartWorkRequest(Guid CrewId);

public record SubmitProgressRequest(decimal? FactQuantity, decimal? FactManDay, decimal? Overtime, string? Comment);

public record RejectRequest(string Reason);
