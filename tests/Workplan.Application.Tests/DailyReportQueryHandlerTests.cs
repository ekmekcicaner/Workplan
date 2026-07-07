using FluentAssertions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Common;
using Workplan.Application.Features.Reports.Queries.GetDailyReport;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.Infrastructure.Persistence;
using Xunit;
using WorkUnit = Workplan.Domain.ValueObjects.Unit;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.Application.Tests;

public class DailyReportQueryHandlerTests
{
    [Fact]
    [Trait("Category", "Application")]
    public async Task GetDailyReport_returns_only_pm_approved_work_and_kpis()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var approved = CreateApprovedPlan(seed, 8, 2, 1);
        var notApproved = CreateSubmittedPlan(seed, 6, 2, null);
        db.DailyPlans.AddRange(approved, notApproved);
        await db.SaveChangesAsync(CancellationToken.None);

        var currentUser = new CurrentUserStub(seed.PmId, [Roles.ProjectManager]);
        var handler = new GetDailyReportQueryHandler(db, new AccessScopeService(db, currentUser));

        var result = await handler.Handle(
            new GetDailyReportQuery(null, seed.Project.Id, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].Id.Should().Be(approved.Id);
        result.Value.Summary.ApprovedWorkCount.Should().Be(1);
        result.Value.Summary.FactManDay.Should().Be(2);
        result.Value.Summary.Overtime.Should().Be(1);
        result.Value.QuantityByUnit.Should().ContainSingle(kpi =>
            kpi.Unit == WorkUnit.M2 && kpi.PlannedQuantity == 10 && kpi.FactQuantity == 8);
        result.Value.QuantityByWorkItem.Should().ContainSingle(kpi =>
            kpi.WorkItemTypeId == seed.WorkItemType.Id
            && kpi.WorkItemTypeName == seed.WorkItemType.Name
            && kpi.Unit == WorkUnit.M2
            && kpi.PlannedQuantity == 10
            && kpi.FactQuantity == 8);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, new NoopPublisher());
    }

    private static async Task<SeedData> SeedAsync(AppDbContext db)
    {
        var pm = Guid.NewGuid();
        var siteChief = Guid.NewGuid();
        var techOffice = Guid.NewGuid();
        var headOfMaster = Guid.NewGuid();

        var project = Project.Create("P1", "Project", pm).Value;
        var region = CrewRegion.Create(project.Id, "R1", "Region").Value;
        region.AssignSiteChief(siteChief);
        region.AssignTechOffice(techOffice);
        var location = Location.Create(project.Id, region.Id, "Location").Value;
        location.AssignHeadOfMaster(headOfMaster);
        var workItemType = WorkItemType.Create("Leaf", Guid.NewGuid(), 1, WorkUnit.M2).Value;

        db.Projects.Add(project);
        db.CrewRegions.Add(region);
        db.Locations.Add(location);
        db.WorkItemTypes.Add(workItemType);
        await db.SaveChangesAsync(CancellationToken.None);

        return new SeedData(pm, siteChief, techOffice, headOfMaster, project, region, location, workItemType);
    }

    private static DailyPlan CreateSubmittedPlan(SeedData seed, decimal factQuantity, decimal factManDay, decimal? overtime)
    {
        var plan = DailyPlan.CreateFromPlan(
            seed.Project.Id,
            seed.Region.Id,
            seed.Location.Id,
            seed.WorkItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            seed.TechOfficeId,
            seed.HeadOfMasterId,
            10,
            3,
            WorkUnit.M2).Value;
        plan.StartWork(seed.HeadOfMasterId, Guid.NewGuid()).IsSuccess.Should().BeTrue();
        plan.SubmitProgress(factQuantity, factManDay, overtime, null, seed.HeadOfMasterId).IsSuccess.Should().BeTrue();
        return plan;
    }

    private static DailyPlan CreateApprovedPlan(SeedData seed, decimal factQuantity, decimal factManDay, decimal overtime)
    {
        var plan = CreateSubmittedPlan(seed, factQuantity, factManDay, overtime);
        plan.Approve(Workplan.Domain.Enums.WorkStatus.ApprovedBySiteChief, seed.Region.Id, seed.SiteChiefId)
            .IsSuccess.Should().BeTrue();
        plan.Approve(Workplan.Domain.Enums.WorkStatus.ApprovedByPM, seed.Project.Id, seed.PmId)
            .IsSuccess.Should().BeTrue();
        return plan;
    }

    private sealed record CurrentUserStub(Guid? UserId, IReadOnlyList<string> Roles) : ICurrentUserService;

    private sealed record SeedData(
        Guid PmId,
        Guid SiteChiefId,
        Guid TechOfficeId,
        Guid HeadOfMasterId,
        Project Project,
        CrewRegion Region,
        Location Location,
        WorkItemType WorkItemType);

    private sealed class NoopPublisher : IPublisher
    {
        public ValueTask Publish(object notification, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;

        public ValueTask Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification => ValueTask.CompletedTask;
    }
}
