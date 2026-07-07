using Workplan.Domain.Enums;
using Workplan.Domain.ValueObjects;

namespace Workplan.Application.Features.DailyPlans;

public sealed record DailyPlanDto(
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

public sealed record StatusTransitionDto(
    WorkStatus FromStatus,
    WorkStatus ToStatus,
    Guid ActionById,
    DateTime TransitionedAt,
    string? ActionByName,
    string? Note);
