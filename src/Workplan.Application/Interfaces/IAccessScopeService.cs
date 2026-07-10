using Workplan.Domain.Entities;

namespace Workplan.Application.Interfaces;

public interface IAccessScopeService
{
    bool IsSystemAdmin { get; }

    IQueryable<Project> ApplyProjectScope(IQueryable<Project> query);
    IQueryable<CrewRegion> ApplyCrewRegionScope(IQueryable<CrewRegion> query);
    IQueryable<Location> ApplyLocationScope(IQueryable<Location> query);
    IQueryable<DailyPlan> ApplyDailyPlanScope(IQueryable<DailyPlan> query);

    Task<bool> CanAccessProjectAsync(Guid projectId, CancellationToken cancellationToken);
    Task<bool> CanAccessCrewRegionAsync(Guid crewRegionId, CancellationToken cancellationToken);
    Task<bool> CanAccessLocationAsync(Guid locationId, CancellationToken cancellationToken);
    Task<bool> CanAccessDailyPlanAsync(Guid dailyPlanId, CancellationToken cancellationToken);

    // Onay/red akışındaki rol-spesifik sahiplik kontrolleri (bkz. ApproveCommandHandler/RejectCommandHandler).
    // CanAccessCrewRegionAsync/CanAccessProjectAsync'ten farkı: burada "herhangi bir yetkili rol" değil,
    // tam olarak o bölgenin Site Chief'i / o projenin PM'i olma şartı aranır.
    Task<bool> IsSiteChiefOfCrewRegionAsync(Guid crewRegionId, CancellationToken cancellationToken);
    Task<bool> IsProjectManagerOfProjectAsync(Guid projectId, CancellationToken cancellationToken);
}
