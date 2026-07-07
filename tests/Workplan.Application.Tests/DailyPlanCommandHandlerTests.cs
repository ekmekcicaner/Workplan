using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Common;
using Workplan.Application.Features.DailyPlans.Commands;
using Workplan.Application.Features.DailyPlans.Queries.GetApprovalQueue;
using Workplan.Application.Features.DailyPlans.Queries.GetMyWorkDailyPlans;
using Workplan.Application.Features.Notifications.Commands;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.Domain.Enums;
using Workplan.Domain.ValueObjects;
using Workplan.Infrastructure.Persistence;
using Xunit;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.Application.Tests;

public class DailyPlanCommandHandlerTests
{
    [Fact]
    [Trait("Category", "Application")]
    public async Task CreateDailyPlan_creates_plan_and_assignment_notification()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var currentUser = new CurrentUserStub(seed.TechOfficeId, [Roles.TechnicalOfficeEngineer]);
        var handler = new CreateDailyPlanCommandHandler(db, currentUser, new AccessScopeService(db, currentUser));

        var result = await handler.Handle(new CreateDailyPlanCommand(
            seed.Project.Id,
            seed.Region.Id,
            seed.Location.Id,
            seed.WorkItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            7,
            2,
            seed.HeadOfMasterId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await db.DailyPlans.CountAsync()).Should().Be(1);
        (await db.Notifications.CountAsync(n => n.UserId == seed.HeadOfMasterId && n.DailyPlanId == result.Value))
            .Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task CreateDailyPlan_rejects_unauthenticated_user()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var currentUser = new CurrentUserStub(null, []);
        var handler = new CreateDailyPlanCommandHandler(db, currentUser, new AccessScopeService(db, currentUser));

        var result = await handler.Handle(new CreateDailyPlanCommand(
            seed.Project.Id,
            seed.Region.Id,
            seed.Location.Id,
            seed.WorkItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            7,
            2,
            seed.HeadOfMasterId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("unauthorized");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task CreateDailyPlan_rejects_assignee_that_is_not_location_head_of_master()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var currentUser = new CurrentUserStub(seed.TechOfficeId, [Roles.TechnicalOfficeEngineer]);
        var handler = new CreateDailyPlanCommandHandler(db, currentUser, new AccessScopeService(db, currentUser));

        var result = await handler.Handle(new CreateDailyPlanCommand(
            seed.Project.Id,
            seed.Region.Id,
            seed.Location.Id,
            seed.WorkItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            7,
            2,
            Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task StartWork_rejects_wrong_head_of_master()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var plan = CreateAssignedPlan(seed);
        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);

        var currentUser = new CurrentUserStub(Guid.NewGuid(), [Roles.HeadOfMaster]);
        var handler = new StartWorkCommandHandler(db, currentUser, new AccessScopeService(db, currentUser));

        var result = await handler.Handle(new StartWorkCommand(plan.Id, seed.CrewType.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("scope_mismatch");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task StartWork_with_worker_type_creates_crew_snapshot_and_SubmitProgress_persists_status_changes()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var plan = CreateAssignedPlan(seed);
        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);

        var currentUser = new CurrentUserStub(seed.HeadOfMasterId, [Roles.HeadOfMaster]);
        var scope = new AccessScopeService(db, currentUser);
        var startResult = await new StartWorkCommandHandler(db, currentUser, scope)
            .Handle(new StartWorkCommand(plan.Id, seed.CrewType.Id), CancellationToken.None);
        var submitResult = await new SubmitProgressCommandHandler(db, currentUser, scope)
            .Handle(new SubmitProgressCommand(plan.Id, 5, 1.5m, null, null), CancellationToken.None);

        startResult.IsSuccess.Should().BeTrue();
        submitResult.IsSuccess.Should().BeTrue();
        var savedPlan = (await db.DailyPlans.FindAsync([plan.Id], CancellationToken.None))!;
        savedPlan.Status.Should().Be(WorkStatus.Submitted);
        savedPlan.CrewTypeId.Should().Be(seed.CrewType.Id);
        (await db.StatusTransitions.CountAsync()).Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Approve_requires_matching_site_chief_scope()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var plan = CreateSubmittedPlan(seed);
        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);

        var wrongSiteChief = new CurrentUserStub(Guid.NewGuid(), [Roles.SiteChief]);
        var wrongResult = await new ApproveCommandHandler(db, wrongSiteChief)
            .Handle(new ApproveCommand(plan.Id), CancellationToken.None);

        wrongResult.IsFailure.Should().BeTrue();
        wrongResult.Error.Code.Should().Be("scope_mismatch");

        var siteChief = new CurrentUserStub(seed.SiteChiefId, [Roles.SiteChief]);
        var result = await new ApproveCommandHandler(db, siteChief)
            .Handle(new ApproveCommand(plan.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await db.DailyPlans.FindAsync([plan.Id], CancellationToken.None))!
            .Status.Should().Be(WorkStatus.ApprovedBySiteChief);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task SiteChief_reject_returns_to_head_of_master_and_creates_rejection_notification()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var plan = CreateSubmittedPlan(seed);
        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);

        var siteChief = new CurrentUserStub(seed.SiteChiefId, [Roles.SiteChief]);
        var result = await new RejectCommandHandler(db, siteChief)
            .Handle(new RejectCommand(plan.Id, "revise quantity"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await db.DailyPlans.FindAsync([plan.Id], CancellationToken.None))!
            .Status.Should().Be(WorkStatus.InProgress);

        var notification = await db.Notifications.SingleAsync(n =>
            n.UserId == seed.HeadOfMasterId
            && n.DailyPlanId == plan.Id
            && n.Type == "DailyPlanRejected");
        notification.Message.Should().Contain("Şantiye Şefi");

        var myWork = await new GetMyWorkDailyPlansQueryHandler(
                db,
                new CurrentUserStub(seed.HeadOfMasterId, [Roles.HeadOfMaster]),
                new AccessScopeService(db, new CurrentUserStub(seed.HeadOfMasterId, [Roles.HeadOfMaster])))
            .Handle(new GetMyWorkDailyPlansQuery(), CancellationToken.None);

        myWork.IsSuccess.Should().BeTrue();
        myWork.Value.Should().ContainSingle(item => item.Id == plan.Id)
            .Which.LatestRejectionReason.Should().Be("revise quantity");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task ProjectManager_reject_returns_to_site_chief_and_creates_rejection_notification()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var plan = CreateSubmittedPlan(seed);
        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);

        var siteChief = new CurrentUserStub(seed.SiteChiefId, [Roles.SiteChief]);
        var approveResult = await new ApproveCommandHandler(db, siteChief)
            .Handle(new ApproveCommand(plan.Id), CancellationToken.None);
        approveResult.IsSuccess.Should().BeTrue();

        var pm = new CurrentUserStub(seed.PmId, [Roles.ProjectManager]);
        var rejectResult = await new RejectCommandHandler(db, pm)
            .Handle(new RejectCommand(plan.Id, "site chief should review"), CancellationToken.None);

        rejectResult.IsSuccess.Should().BeTrue();
        (await db.DailyPlans.FindAsync([plan.Id], CancellationToken.None))!
            .Status.Should().Be(WorkStatus.Submitted);

        var notification = await db.Notifications.SingleAsync(n =>
            n.UserId == seed.SiteChiefId
            && n.DailyPlanId == plan.Id
            && n.Type == "DailyPlanRejected");
        notification.Message.Should().Contain("Project Manager");

        var siteChiefUser = new CurrentUserStub(seed.SiteChiefId, [Roles.SiteChief]);
        var approvalQueue = await new GetApprovalQueueQueryHandler(
                db,
                siteChiefUser,
                new AccessScopeService(db, siteChiefUser))
            .Handle(new GetApprovalQueueQuery(), CancellationToken.None);

        approvalQueue.IsSuccess.Should().BeTrue();
        var item = approvalQueue.Value.Should().ContainSingle(row => row.Id == plan.Id).Which;
        item.LatestRejectionReason.Should().Be("site chief should review");
        item.LatestRejectionFromStatus.Should().Be(WorkStatus.ApprovedBySiteChief);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task MarkDailyPlanNotificationsRead_marks_only_current_users_matching_notifications()
    {
        await using var db = CreateDbContext();
        var user = Guid.NewGuid();
        var otherUser = Guid.NewGuid();
        var dailyPlanId = Guid.NewGuid();
        var mine = Notification.CreateDailyPlanAssigned(user, dailyPlanId, DateOnly.FromDateTime(DateTime.UtcNow)).Value;
        var theirs = Notification.CreateDailyPlanAssigned(otherUser, dailyPlanId, DateOnly.FromDateTime(DateTime.UtcNow)).Value;
        db.Notifications.AddRange(mine, theirs);
        await db.SaveChangesAsync(CancellationToken.None);

        var result = await new MarkDailyPlanNotificationsReadCommandHandler(db, new CurrentUserStub(user, []))
            .Handle(new MarkDailyPlanNotificationsReadCommand(dailyPlanId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        mine.ReadAtUtc.Should().NotBeNull();
        theirs.ReadAtUtc.Should().BeNull();
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, null!);
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
        var workItemType = WorkItemType.Create("Leaf", Guid.NewGuid(), 1, Unit.M2).Value;
        var crewType = CrewType.Create("Düz işçi").Value;

        db.Projects.Add(project);
        db.CrewRegions.Add(region);
        db.Locations.Add(location);
        db.WorkItemTypes.Add(workItemType);
        db.CrewTypes.Add(crewType);
        await db.SaveChangesAsync(CancellationToken.None);

        return new SeedData(pm, siteChief, techOffice, headOfMaster, project, region, location, workItemType, crewType);
    }

    private static DailyPlan CreateAssignedPlan(SeedData seed) =>
        DailyPlan.CreateFromPlan(
            seed.Project.Id,
            seed.Region.Id,
            seed.Location.Id,
            seed.WorkItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            seed.TechOfficeId,
            seed.HeadOfMasterId,
            10,
            2,
            Unit.M2).Value;

    private static DailyPlan CreateSubmittedPlan(SeedData seed)
    {
        var plan = CreateAssignedPlan(seed);
        plan.StartWork(seed.HeadOfMasterId, Guid.NewGuid()).IsSuccess.Should().BeTrue();
        plan.SubmitProgress(5, 1, null, null, seed.HeadOfMasterId).IsSuccess.Should().BeTrue();
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
        WorkItemType WorkItemType,
        CrewType CrewType);
}
