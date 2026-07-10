using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Enums;
using Workplan.SharedKernel.Common;

namespace Workplan.Application.Features.Reports.Queries.GetDailyReport;

public class GetDailyReportQueryHandler(IApplicationDbContext db, IAccessScopeService accessScope)
    : IRequestHandler<GetDailyReportQuery, Result<DailyReportDto>>
{
    public async ValueTask<Result<DailyReportDto>> Handle(
        GetDailyReportQuery request,
        CancellationToken cancellationToken)
    {
        var query = accessScope.ApplyDailyPlanScope(db.DailyPlans.AsNoTracking())
            .Where(plan => plan.Status == WorkStatus.ApprovedByPM);

        if (request.WorkDate is { } workDate)
            query = query.Where(plan => plan.WorkDate == workDate);
        if (request.ProjectId is { } projectId)
            query = query.Where(plan => plan.ProjectId == projectId);
        if (request.CrewRegionId is { } crewRegionId)
            query = query.Where(plan => plan.CrewRegionId == crewRegionId);
        if (request.HeadOfMasterId is { } headOfMasterId)
            query = query.Where(plan => plan.AssignedHoMId == headOfMasterId);

        var items = await (
            from plan in query
            join project in db.Projects.AsNoTracking() on plan.ProjectId equals project.Id
            join region in db.CrewRegions.AsNoTracking() on plan.CrewRegionId equals region.Id
            join location in db.Locations.AsNoTracking() on plan.LocationId equals location.Id
            join workItemType in db.WorkItemTypes.AsNoTracking() on plan.WorkItemTypeId equals workItemType.Id
            join crewType in db.CrewTypes.AsNoTracking() on plan.CrewTypeId equals (Guid?)crewType.Id into crewTypeJoin
            from crewType in crewTypeJoin.DefaultIfEmpty()
            orderby plan.WorkDate descending, project.Name, region.Name, location.Name
            select new DailyReportItemDto(
                plan.Id,
                plan.WorkDate,
                plan.ProjectId,
                project.Name,
                plan.CrewRegionId,
                region.Name,
                plan.LocationId,
                location.Name,
                plan.WorkItemTypeId,
                workItemType.Name,
                plan.AssignedHoMId,
                crewType == null ? null : crewType.Name,
                plan.PlannedQuantity,
                plan.PlannedManDay,
                plan.Unit,
                plan.FactQuantity ?? 0,
                plan.FactManDay ?? 0,
                plan.Overtime ?? 0,
                plan.Comment,
                plan.Status))
            .ToListAsync(cancellationToken);

        var plannedManDay = items.Sum(item => item.PlannedManDay);
        var factManDay = items.Sum(item => item.FactManDay);
        var overtime = items.Sum(item => item.Overtime);
        var summary = new DailyReportSummaryDto(
            items.Count,
            plannedManDay,
            factManDay,
            overtime,
            plannedManDay == 0 ? null : factManDay / plannedManDay);

        var quantityByUnit = items
            .GroupBy(item => item.Unit)
            .OrderBy(group => group.Key)
            .Select(group => new UnitQuantityKpiDto(
                group.Key,
                group.Sum(item => item.PlannedQuantity),
                group.Sum(item => item.FactQuantity)))
            .ToList();

        var quantityByWorkItem = items
            .GroupBy(item => new { item.WorkItemTypeId, item.WorkItemTypeName, item.Unit })
            .OrderBy(group => group.Key.WorkItemTypeName)
            .Select(group => new WorkItemQuantityKpiDto(
                group.Key.WorkItemTypeId,
                group.Key.WorkItemTypeName,
                group.Key.Unit,
                group.Sum(item => item.PlannedQuantity),
                group.Sum(item => item.FactQuantity)))
            .ToList();

        return new DailyReportDto(summary, quantityByUnit, quantityByWorkItem, items);
    }
}
