using Microsoft.EntityFrameworkCore;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.Application.Common;

public class AccessScopeService : IAccessScopeService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AccessScopeService(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

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
        CanViewAll ? query : ApplyDailyPlanOwnershipFilter(query);

    public IQueryable<Crew> ApplyCrewScope(IQueryable<Crew> query) =>
        CanViewAll ? query : ApplyCrewOwnershipFilter(query);

    public Task<bool> CanAccessProjectAsync(Guid projectId, CancellationToken cancellationToken) =>
        ApplyProjectOwnershipFilter(_db.Projects.AsNoTracking())
            .AnyAsync(p => p.Id == projectId, cancellationToken);

    public Task<bool> CanAccessCrewRegionAsync(Guid crewRegionId, CancellationToken cancellationToken) =>
        ApplyCrewRegionOwnershipFilter(_db.CrewRegions.AsNoTracking())
            .AnyAsync(r => r.Id == crewRegionId, cancellationToken);

    public Task<bool> CanAccessLocationAsync(Guid locationId, CancellationToken cancellationToken) =>
        ApplyLocationOwnershipFilter(_db.Locations.AsNoTracking())
            .AnyAsync(l => l.Id == locationId, cancellationToken);

    public Task<bool> CanAccessDailyPlanAsync(Guid dailyPlanId, CancellationToken cancellationToken) =>
        ApplyDailyPlanOwnershipFilter(_db.DailyPlans.AsNoTracking())
            .AnyAsync(p => p.Id == dailyPlanId, cancellationToken);

    public Task<bool> CanAccessCrewAsync(Guid crewId, CancellationToken cancellationToken) =>
        ApplyCrewOwnershipFilter(_db.Crews.AsNoTracking())
            .AnyAsync(c => c.Id == crewId, cancellationToken);

    // Aşağıdaki filtreler işlem (atama, onay, ekip/plan oluşturma vb.) yetkisini belirler:
    // proje yöneticisi ve teknik ofis de dahil olmak üzere herkes yalnızca kendi
    // projesi/bölgesi/lokasyonu üzerinde işlem yapabilir.
    private IQueryable<Project> ApplyProjectOwnershipFilter(IQueryable<Project> query)
    {
        if (IsSystemAdmin) return query;
        if (_currentUser.UserId is not { } userId) return query.Where(_ => false);

        var isProjectManager = HasRole(Roles.ProjectManager);
        var isSiteChief = HasRole(Roles.SiteChief);
        var isTechOffice = HasRole(Roles.TechnicalOfficeEngineer);
        var isHeadOfMaster = HasRole(Roles.HeadOfMaster);

        return query.Where(p =>
            (isProjectManager && p.PmUserId == userId)
            || (isSiteChief && _db.CrewRegions.Any(r =>
                r.ProjectId == p.Id && r.SiteChiefUserId == userId))
            || (isTechOffice && _db.CrewRegions.Any(r =>
                r.ProjectId == p.Id && r.TechOfficeUserId == userId))
            || (isHeadOfMaster && _db.Locations.Any(l =>
                l.ProjectId == p.Id && l.HeadOfMasterUserId == userId))
            || (isHeadOfMaster && _db.DailyPlans.Any(d =>
                d.ProjectId == p.Id && d.AssignedHoMId == userId)));
    }

    private IQueryable<CrewRegion> ApplyCrewRegionOwnershipFilter(IQueryable<CrewRegion> query)
    {
        if (IsSystemAdmin) return query;
        if (_currentUser.UserId is not { } userId) return query.Where(_ => false);

        var isProjectManager = HasRole(Roles.ProjectManager);
        var isSiteChief = HasRole(Roles.SiteChief);
        var isTechOffice = HasRole(Roles.TechnicalOfficeEngineer);
        var isHeadOfMaster = HasRole(Roles.HeadOfMaster);

        return query.Where(r =>
            (isProjectManager && _db.Projects.Any(p =>
                p.Id == r.ProjectId && p.PmUserId == userId))
            || (isSiteChief && r.SiteChiefUserId == userId)
            || (isTechOffice && r.TechOfficeUserId == userId)
            || (isHeadOfMaster && _db.Locations.Any(l =>
                l.CrewRegionId == r.Id && l.HeadOfMasterUserId == userId))
            || (isHeadOfMaster && _db.DailyPlans.Any(d =>
                d.CrewRegionId == r.Id && d.AssignedHoMId == userId)));
    }

    private IQueryable<Location> ApplyLocationOwnershipFilter(IQueryable<Location> query)
    {
        if (IsSystemAdmin) return query;
        if (_currentUser.UserId is not { } userId) return query.Where(_ => false);

        var isProjectManager = HasRole(Roles.ProjectManager);
        var isSiteChief = HasRole(Roles.SiteChief);
        var isTechOffice = HasRole(Roles.TechnicalOfficeEngineer);
        var isHeadOfMaster = HasRole(Roles.HeadOfMaster);

        return query.Where(l =>
            (isProjectManager && _db.Projects.Any(p =>
                p.Id == l.ProjectId && p.PmUserId == userId))
            || (isSiteChief && _db.CrewRegions.Any(r =>
                r.Id == l.CrewRegionId && r.SiteChiefUserId == userId))
            || (isTechOffice && _db.CrewRegions.Any(r =>
                r.Id == l.CrewRegionId && r.TechOfficeUserId == userId))
            || (isHeadOfMaster && l.HeadOfMasterUserId == userId)
            || (isHeadOfMaster && _db.DailyPlans.Any(d =>
                d.LocationId == l.Id && d.AssignedHoMId == userId)));
    }

    private IQueryable<DailyPlan> ApplyDailyPlanOwnershipFilter(IQueryable<DailyPlan> query)
    {
        if (IsSystemAdmin) return query;
        if (_currentUser.UserId is not { } userId) return query.Where(_ => false);

        var isProjectManager = HasRole(Roles.ProjectManager);
        var isSiteChief = HasRole(Roles.SiteChief);
        var isTechOffice = HasRole(Roles.TechnicalOfficeEngineer);
        var isHeadOfMaster = HasRole(Roles.HeadOfMaster);

        return query.Where(p =>
            (isProjectManager && _db.Projects.Any(project =>
                project.Id == p.ProjectId && project.PmUserId == userId))
            || (isSiteChief && _db.CrewRegions.Any(r =>
                r.Id == p.CrewRegionId && r.SiteChiefUserId == userId))
            || (isTechOffice && _db.CrewRegions.Any(r =>
                r.Id == p.CrewRegionId && r.TechOfficeUserId == userId))
            || (isHeadOfMaster && p.AssignedHoMId == userId));
    }

    private IQueryable<Crew> ApplyCrewOwnershipFilter(IQueryable<Crew> query)
    {
        if (IsSystemAdmin) return query;
        if (_currentUser.UserId is not { } userId) return query.Where(_ => false);

        var isProjectManager = HasRole(Roles.ProjectManager);
        var isSiteChief = HasRole(Roles.SiteChief);
        var isTechOffice = HasRole(Roles.TechnicalOfficeEngineer);
        var isHeadOfMaster = HasRole(Roles.HeadOfMaster);

        return query.Where(c =>
            (isProjectManager && _db.Locations.Any(l =>
                l.Id == c.LocationId && _db.Projects.Any(p =>
                    p.Id == l.ProjectId && p.PmUserId == userId)))
            || (isSiteChief && _db.Locations.Any(l =>
                l.Id == c.LocationId && _db.CrewRegions.Any(r =>
                    r.Id == l.CrewRegionId && r.SiteChiefUserId == userId)))
            || (isTechOffice && _db.Locations.Any(l =>
                l.Id == c.LocationId && _db.CrewRegions.Any(r =>
                    r.Id == l.CrewRegionId && r.TechOfficeUserId == userId)))
            || (isHeadOfMaster && c.CreatedByHoMId == userId && _db.Locations.Any(l =>
                l.Id == c.LocationId && l.HeadOfMasterUserId == userId)));
    }

    private bool HasRole(string role) => _currentUser.Roles.Contains(role);
}
