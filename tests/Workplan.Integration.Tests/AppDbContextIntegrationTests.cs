using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Workplan.Domain.Entities;
using Workplan.Domain.Enums;
using Workplan.Domain.ValueObjects;
using Workplan.Integration.Tests.TestInfrastructure;
using Xunit;

namespace Workplan.Integration.Tests;

public class AppDbContextIntegrationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public AppDbContextIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Migrations_persist_daily_plan_graph_and_status_history_in_postgres()
    {
        if (!DockerTestGuard.Enabled)
            return;

        await using var db = _fixture.CreateDbContext();
        var seed = SeedDailyPlan();

        db.Projects.Add(seed.Project);
        db.CrewRegions.Add(seed.Region);
        db.Locations.Add(seed.Location);
        db.WorkItemTypes.Add(seed.WorkItemType);
        db.DailyPlans.Add(seed.Plan);
        db.StatusTransitions.Add(seed.Plan.History.Last());

        await db.SaveChangesAsync(CancellationToken.None);

        await using var verifyDb = _fixture.CreateDbContext();
        var savedPlan = await verifyDb.DailyPlans
            .Include(p => p.History)
            .SingleAsync(p => p.Id == seed.Plan.Id);

        savedPlan.Status.Should().Be(WorkStatus.Assigned);
        savedPlan.Unit.Should().Be(Unit.M2);
        savedPlan.History.Should().ContainSingle(h => h.ToStatus == WorkStatus.Assigned);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Query_filters_can_exclude_inactive_projects_against_postgres()
    {
        if (!DockerTestGuard.Enabled)
            return;

        await using var db = _fixture.CreateDbContext();
        var active = Project.Create($"A-{Guid.NewGuid():N}", "Active").Value;
        var inactive = Project.Create($"I-{Guid.NewGuid():N}", "Inactive").Value;
        inactive.Deactivate();

        db.Projects.AddRange(active, inactive);
        await db.SaveChangesAsync(CancellationToken.None);

        var activeProjectIds = await db.Projects
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => p.Id)
            .ToListAsync();

        activeProjectIds.Should().Contain(active.Id);
        activeProjectIds.Should().NotContain(inactive.Id);
    }

    private static SeedData SeedDailyPlan()
    {
        var pm = Guid.NewGuid();
        var tech = Guid.NewGuid();
        var hom = Guid.NewGuid();

        var project = Project.Create($"P-{Guid.NewGuid():N}", "Project", pm).Value;
        var region = CrewRegion.Create(project.Id, $"R-{Guid.NewGuid():N}", "Region").Value;
        region.AssignTechOffice(tech);
        var location = Location.Create(project.Id, region.Id, "Location").Value;
        location.AssignHeadOfMaster(hom);
        var workItemType = WorkItemType.Create("Leaf", Guid.NewGuid(), 1, Unit.M2).Value;
        var plan = DailyPlan.CreateFromPlan(
            project.Id,
            region.Id,
            location.Id,
            workItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            tech,
            hom,
            10,
            2,
            Unit.M2).Value;

        return new SeedData(project, region, location, workItemType, plan);
    }

    private sealed record SeedData(
        Project Project,
        CrewRegion Region,
        Location Location,
        WorkItemType WorkItemType,
        DailyPlan Plan);
}
