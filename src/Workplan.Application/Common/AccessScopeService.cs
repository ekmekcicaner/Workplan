using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.Application.Common;

public class AccessScopeService(IApplicationDbContext db, ICurrentUserService currentUser) : IAccessScopeService
{
    public bool IsSystemAdmin => HasRole(Roles.SystemAdmin);

    // Proje yöneticisi ve teknik ofis her şeyi görüntüleyebilir; işlem yetkisi ayrıca
    // *OwnershipFilter metotlarıyla kendi iş/proje/bölgeleriyle sınırlanır.
    private bool CanViewAll => IsSystemAdmin
        || HasRole(Roles.ProjectManager)
        || HasRole(Roles.TechnicalOfficeEngineer);

    public IQueryable<Project> ApplyProjectScope(IQueryable<Project> query) =>
        CanViewAll ? query : ApplyProjectOwnershipFilter(query);

    public IQueryable<CrewRegion> ApplyCrewRegionScope(IQueryable<CrewRegion> query) =>
        CanViewAll ? query : ApplyCrewRegionOwnershipFilter(query);

    public IQueryable<Location> ApplyLocationScope(IQueryable<Location> query) =>
        CanViewAll ? query : ApplyLocationOwnershipFilter(query);

    public IQueryable<DailyPlan> ApplyDailyPlanScope(IQueryable<DailyPlan> query) =>
        IsSystemAdmin || HasRole(Roles.TechnicalOfficeEngineer)
            ? query
            : ApplyDailyPlanOwnershipFilter(query);

    public Task<bool> CanAccessProjectAsync(Guid projectId, CancellationToken cancellationToken) =>
        ApplyProjectOwnershipFilter(db.Projects.AsNoTracking())
            .AnyAsync(p => p.Id == projectId, cancellationToken);

    public Task<bool> CanAccessCrewRegionAsync(Guid crewRegionId, CancellationToken cancellationToken) =>
        ApplyCrewRegionOwnershipFilter(db.CrewRegions.AsNoTracking())
            .AnyAsync(r => r.Id == crewRegionId, cancellationToken);

    public Task<bool> CanAccessLocationAsync(Guid locationId, CancellationToken cancellationToken) =>
        ApplyLocationOwnershipFilter(db.Locations.AsNoTracking())
            .AnyAsync(l => l.Id == locationId, cancellationToken);

    public Task<bool> CanAccessDailyPlanAsync(Guid dailyPlanId, CancellationToken cancellationToken) =>
        ApplyDailyPlanOwnershipFilter(db.DailyPlans.AsNoTracking())
            .AnyAsync(p => p.Id == dailyPlanId, cancellationToken);

    public Task<bool> IsSiteChiefOfCrewRegionAsync(Guid crewRegionId, CancellationToken cancellationToken) =>
        currentUser.UserId is { } userId
            ? db.CrewRegions.AsNoTracking()
                .AnyAsync(r => r.Id == crewRegionId && r.SiteChiefUserId == userId, cancellationToken)
            : Task.FromResult(false);

    public Task<bool> IsProjectManagerOfProjectAsync(Guid projectId, CancellationToken cancellationToken) =>
        currentUser.UserId is { } userId
            ? db.Projects.AsNoTracking()
                .AnyAsync(p => p.Id == projectId && p.PmUserId == userId, cancellationToken)
            : Task.FromResult(false);

    // Aşağıdaki filtreler işlem (atama, onay, ekip/plan oluşturma vb.) yetkisini belirler:
    // proje yöneticisi ve teknik ofis de dahil olmak üzere herkes yalnızca kendi
    // projesi/bölgesi/lokasyonu üzerinde işlem yapabilir. Guard (admin/kimliksiz) ve rol
    // bayrakları ResolveOwnership() içinde tek yerden çözülür; her entity'nin gezinme yolu
    // (navigation path) farklı olduğundan predicate'lerin kendisi ayrı kalır.
    private (bool Bypass, OwnershipContext? Context) ResolveOwnership()
    {
        if (IsSystemAdmin) return (true, null);
        if (currentUser.UserId is not { } userId) return (false, null);

        return (false, new OwnershipContext(
            userId,
            HasRole(Roles.ProjectManager),
            HasRole(Roles.SiteChief),
            HasRole(Roles.TechnicalOfficeEngineer),
            HasRole(Roles.HeadOfMaster)));
    }

    private IQueryable<Project> ApplyProjectOwnershipFilter(IQueryable<Project> query)
    {
        var (bypass, context) = ResolveOwnership();
        if (bypass) return query;
        if (context is not { } c) return query.Where(_ => false);

        return query.Where(p =>
            (c.IsProjectManager && p.PmUserId == c.UserId)
            || (c.IsSiteChief && db.CrewRegions.Any(r =>
                r.ProjectId == p.Id && r.SiteChiefUserId == c.UserId))
            || (c.IsTechOffice && db.CrewRegions.Any(r =>
                r.ProjectId == p.Id && r.TechOfficeUserId == c.UserId))
            || (c.IsHeadOfMaster && db.Locations.Any(l =>
                l.ProjectId == p.Id && l.HeadOfMasterUserId == c.UserId))
            || (c.IsHeadOfMaster && db.DailyPlans.Any(d =>
                d.ProjectId == p.Id && d.AssignedHoMId == c.UserId)));
    }

    private IQueryable<CrewRegion> ApplyCrewRegionOwnershipFilter(IQueryable<CrewRegion> query)
    {
        var (bypass, context) = ResolveOwnership();
        if (bypass) return query;
        if (context is not { } c) return query.Where(_ => false);

        return query.Where(r =>
            (c.IsProjectManager && db.Projects.Any(p =>
                p.Id == r.ProjectId && p.PmUserId == c.UserId))
            || (c.IsSiteChief && r.SiteChiefUserId == c.UserId)
            || (c.IsTechOffice && r.TechOfficeUserId == c.UserId)
            || (c.IsHeadOfMaster && db.Locations.Any(l =>
                l.CrewRegionId == r.Id && l.HeadOfMasterUserId == c.UserId))
            || (c.IsHeadOfMaster && db.DailyPlans.Any(d =>
                d.CrewRegionId == r.Id && d.AssignedHoMId == c.UserId)));
    }

    private IQueryable<Location> ApplyLocationOwnershipFilter(IQueryable<Location> query)
    {
        var (bypass, context) = ResolveOwnership();
        if (bypass) return query;
        if (context is not { } c) return query.Where(_ => false);

        return query.Where(l =>
            (c.IsProjectManager && db.Projects.Any(p =>
                p.Id == l.ProjectId && p.PmUserId == c.UserId))
            || (c.IsSiteChief && db.CrewRegions.Any(r =>
                r.Id == l.CrewRegionId && r.SiteChiefUserId == c.UserId))
            || (c.IsTechOffice && db.CrewRegions.Any(r =>
                r.Id == l.CrewRegionId && r.TechOfficeUserId == c.UserId))
            || (c.IsHeadOfMaster && l.HeadOfMasterUserId == c.UserId)
            || (c.IsHeadOfMaster && db.DailyPlans.Any(d =>
                d.LocationId == l.Id && d.AssignedHoMId == c.UserId)));
    }

    private IQueryable<DailyPlan> ApplyDailyPlanOwnershipFilter(IQueryable<DailyPlan> query)
    {
        var (bypass, context) = ResolveOwnership();
        if (bypass) return query;
        if (context is not { } c) return query.Where(_ => false);

        return query.Where(p =>
            (c.IsProjectManager && db.Projects.Any(project =>
                project.Id == p.ProjectId && project.PmUserId == c.UserId))
            || (c.IsSiteChief && db.CrewRegions.Any(r =>
                r.Id == p.CrewRegionId && r.SiteChiefUserId == c.UserId))
            || (c.IsTechOffice && db.CrewRegions.Any(r =>
                r.Id == p.CrewRegionId && r.TechOfficeUserId == c.UserId))
            || (c.IsHeadOfMaster && p.AssignedHoMId == c.UserId));
    }

    private bool HasRole(string role) => currentUser.Roles.Contains(role);

    private readonly record struct OwnershipContext(
        Guid UserId, bool IsProjectManager, bool IsSiteChief, bool IsTechOffice, bool IsHeadOfMaster);
}
