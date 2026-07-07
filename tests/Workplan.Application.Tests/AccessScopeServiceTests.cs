using Microsoft.EntityFrameworkCore;
using Workplan.Application.Common;
using Workplan.Application.Features.CrewRegions.Commands;
using Workplan.Application.Features.DailyPlans.Commands;
using Workplan.Application.Features.Locations.Commands;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.Domain.ValueObjects;
using Workplan.Infrastructure.Persistence;
using Xunit;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.Application.Tests;

public class AccessScopeServiceTests
{
    [Fact]
    public async Task SiteChief_sees_only_assigned_regions()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var scope = CreateScope(db, seed.SiteChiefA, Roles.SiteChief);

        var regions = await scope.ApplyCrewRegionScope(db.CrewRegions.AsNoTracking())
            .OrderBy(r => r.Code)
            .ToListAsync();

        Assert.Single(regions);
        Assert.Equal(seed.RegionA.Id, regions[0].Id);
    }

    [Fact]
    public async Task ProjectManager_sees_all_projects_but_cannot_access_others()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var scope = CreateScope(db, seed.PmA, Roles.ProjectManager);

        var projects = await scope.ApplyProjectScope(db.Projects.AsNoTracking())
            .OrderBy(p => p.Code)
            .ToListAsync();

        Assert.Equal(2, projects.Count);
        Assert.False(await scope.CanAccessProjectAsync(seed.ProjectB.Id, CancellationToken.None));
        Assert.True(await scope.CanAccessProjectAsync(seed.ProjectA.Id, CancellationToken.None));
    }

    [Fact]
    public async Task TechOffice_sees_all_regions_but_cannot_access_others()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var scope = CreateScope(db, seed.TechA, Roles.TechnicalOfficeEngineer);

        var regions = await scope.ApplyCrewRegionScope(db.CrewRegions.AsNoTracking())
            .OrderBy(r => r.Code)
            .ToListAsync();

        Assert.Equal(2, regions.Count);
        Assert.False(await scope.CanAccessCrewRegionAsync(seed.RegionB.Id, CancellationToken.None));
        Assert.True(await scope.CanAccessCrewRegionAsync(seed.RegionA.Id, CancellationToken.None));
    }

    [Fact]
    public async Task SystemAdmin_sees_all_regions()
    {
        await using var db = CreateDbContext();
        await SeedAsync(db);
        var scope = CreateScope(db, Guid.NewGuid(), Roles.SystemAdmin);

        var regions = await scope.ApplyCrewRegionScope(db.CrewRegions.AsNoTracking()).ToListAsync();

        Assert.Equal(2, regions.Count);
    }

    [Fact]
    public async Task TechOffice_cannot_create_daily_plan_outside_own_region()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var currentUser = new CurrentUserStub(seed.TechA, [Roles.TechnicalOfficeEngineer]);
        var scope = new AccessScopeService(db, currentUser);
        var handler = new CreateDailyPlanCommandHandler(db, currentUser, scope);

        var result = await handler.Handle(new CreateDailyPlanCommand(
            seed.ProjectB.Id,
            seed.RegionB.Id,
            seed.LocationB.Id,
            seed.WorkItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.Date),
            10,
            2,
            seed.HomB), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("scope_mismatch", result.Error.Code);
    }

    [Fact]
    public async Task TechOffice_can_create_daily_plan_in_own_region()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var currentUser = new CurrentUserStub(seed.TechA, [Roles.TechnicalOfficeEngineer]);
        var scope = new AccessScopeService(db, currentUser);
        var handler = new CreateDailyPlanCommandHandler(db, currentUser, scope);

        var result = await handler.Handle(new CreateDailyPlanCommand(
            seed.ProjectA.Id,
            seed.RegionA.Id,
            seed.LocationA.Id,
            seed.WorkItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.Date),
            10,
            2,
            seed.HomA), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SiteChief_cannot_assign_head_of_master_outside_own_region()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var scope = CreateScope(db, seed.SiteChiefA, Roles.SiteChief);
        var handler = new AssignHeadOfMasterCommandHandler(db, scope);

        var result = await handler.Handle(
            new AssignHeadOfMasterCommand(seed.LocationB.Id, seed.HomA),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("scope_mismatch", result.Error.Code);
    }

    [Fact]
    public async Task TechOffice_cannot_assign_tech_office_outside_own_region()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var scope = CreateScope(db, seed.TechA, Roles.TechnicalOfficeEngineer);
        var handler = new AssignTechOfficeCommandHandler(db, scope);

        var result = await handler.Handle(
            new AssignTechOfficeCommand(seed.RegionB.Id, seed.TechA),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("scope_mismatch", result.Error.Code);
    }

    [Fact]
    public async Task TechOffice_cannot_assign_site_chief_outside_own_region()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var scope = CreateScope(db, seed.TechA, Roles.TechnicalOfficeEngineer);
        var handler = new AssignSiteChiefCommandHandler(db, scope);

        var result = await handler.Handle(
            new AssignSiteChiefCommand(seed.RegionB.Id, seed.SiteChiefA),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("scope_mismatch", result.Error.Code);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, null!);
    }

    private static IAccessScopeService CreateScope(AppDbContext db, Guid userId, string role) =>
        new AccessScopeService(db, new CurrentUserStub(userId, [role]));

    private static async Task<SeedData> SeedAsync(AppDbContext db)
    {
        var pmA = Guid.NewGuid();
        var pmB = Guid.NewGuid();
        var siteChiefA = Guid.NewGuid();
        var siteChiefB = Guid.NewGuid();
        var techA = Guid.NewGuid();
        var techB = Guid.NewGuid();
        var homA = Guid.NewGuid();
        var homB = Guid.NewGuid();

        var projectA = Project.Create("A", "Project A", pmA).Value;
        var projectB = Project.Create("B", "Project B", pmB).Value;

        var regionA = CrewRegion.Create(projectA.Id, "A-1", "Region A").Value;
        regionA.AssignSiteChief(siteChiefA);
        regionA.AssignTechOffice(techA);

        var regionB = CrewRegion.Create(projectB.Id, "B-1", "Region B").Value;
        regionB.AssignSiteChief(siteChiefB);
        regionB.AssignTechOffice(techB);

        var locationA = Location.Create(projectA.Id, regionA.Id, "Location A").Value;
        locationA.AssignHeadOfMaster(homA);
        var locationB = Location.Create(projectB.Id, regionB.Id, "Location B").Value;
        locationB.AssignHeadOfMaster(homB);

        var workItemType = WorkItemType.Create("Leaf", Guid.NewGuid(), 1, Unit.M2).Value;

        var planA = DailyPlan.CreateFromPlan(
            projectA.Id, regionA.Id, locationA.Id, workItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.Date), techA, homA, 1, 1, Unit.M2).Value;
        var planB = DailyPlan.CreateFromPlan(
            projectB.Id, regionB.Id, locationB.Id, workItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.Date), techB, homB, 1, 1, Unit.M2).Value;

        db.Projects.AddRange(projectA, projectB);
        db.CrewRegions.AddRange(regionA, regionB);
        db.Locations.AddRange(locationA, locationB);
        db.WorkItemTypes.Add(workItemType);
        db.DailyPlans.AddRange(planA, planB);
        await db.SaveChangesAsync(CancellationToken.None);

        return new SeedData(
            pmA, pmB, siteChiefA, siteChiefB, techA, techB, homA, homB,
            projectA, projectB, regionA, regionB, locationA, locationB, workItemType);
    }

    private sealed record CurrentUserStub(Guid? UserId, IReadOnlyList<string> Roles) : ICurrentUserService;

    private sealed record SeedData(
        Guid PmA,
        Guid PmB,
        Guid SiteChiefA,
        Guid SiteChiefB,
        Guid TechA,
        Guid TechB,
        Guid HomA,
        Guid HomB,
        Project ProjectA,
        Project ProjectB,
        CrewRegion RegionA,
        CrewRegion RegionB,
        Location LocationA,
        Location LocationB,
        WorkItemType WorkItemType);
}
