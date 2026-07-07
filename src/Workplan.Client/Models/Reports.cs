namespace Workplan.Client.Models;

public record DailyReportDto(
    DailyReportSummaryDto Summary,
    IReadOnlyList<UnitQuantityKpiDto> QuantityByUnit,
    IReadOnlyList<WorkItemQuantityKpiDto> QuantityByWorkItem,
    IReadOnlyList<DailyReportItemDto> Items);

public record DailyReportSummaryDto(
    int ApprovedWorkCount,
    decimal PlannedManDay,
    decimal FactManDay,
    decimal Overtime,
    decimal? ManDayRealizationRatio);

public record UnitQuantityKpiDto(Unit Unit, decimal PlannedQuantity, decimal FactQuantity);

public record WorkItemQuantityKpiDto(
    Guid WorkItemTypeId,
    string WorkItemTypeName,
    Unit Unit,
    decimal PlannedQuantity,
    decimal FactQuantity);

public record DailyReportItemDto(
    Guid Id,
    DateOnly WorkDate,
    Guid ProjectId,
    string ProjectName,
    Guid CrewRegionId,
    string CrewRegionName,
    Guid LocationId,
    string LocationName,
    Guid WorkItemTypeId,
    string WorkItemTypeName,
    Guid? AssignedHoMId,
    string? CrewTypeName,
    decimal PlannedQuantity,
    decimal PlannedManDay,
    Unit Unit,
    decimal FactQuantity,
    decimal FactManDay,
    decimal Overtime,
    string? Comment,
    WorkStatus Status);
