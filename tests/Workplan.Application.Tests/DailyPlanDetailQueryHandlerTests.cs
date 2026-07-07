using FluentAssertions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Common;
using Workplan.Application.Features.DailyPlans;
using Workplan.Application.Features.DailyPlans.Queries.GetDailyPlanDetail;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.Domain.Enums;
using Workplan.Domain.ValueObjects;
using Workplan.Infrastructure.Persistence;
using Workplan.SharedKernel.Common;
using Xunit;
using WorkUnit = Workplan.Domain.ValueObjects.Unit;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.Application.Tests;

public class DailyPlanDetailQueryHandlerTests
{
    [Fact]
    [Trait("Category", "Application")]
    public async Task GetDailyPlanDetail_returns_responsible_people_and_history_actor_names()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var plan = DailyPlan.CreateFromPlan(
            seed.Project.Id,
            seed.Region.Id,
            seed.Location.Id,
            seed.WorkItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            seed.TechOfficeId,
            seed.HeadOfMasterId,
            10,
            2,
            WorkUnit.M2).Value;
        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);

        var currentUser = new CurrentUserStub(seed.PmId, [Roles.ProjectManager]);
        var identity = new IdentityStub(new Dictionary<Guid, string>
        {
            [seed.PmId] = "Mehmet Kaya",
            [seed.SiteChiefId] = "Ayşe Demir",
            [seed.HeadOfMasterId] = "Mustafa Arslan",
            [seed.TechOfficeId] = "Teknik Ofis"
        });
        var handler = new GetDailyPlanDetailQueryHandler(db, new AccessScopeService(db, currentUser), identity);

        var result = await handler.Handle(new GetDailyPlanDetailQuery(plan.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AssignedHoMName.Should().Be("Mustafa Arslan");
        result.Value.SiteChiefName.Should().Be("Ayşe Demir");
        result.Value.ProjectManagerName.Should().Be("Mehmet Kaya");
        result.Value.History.Should().ContainSingle().Which.ActionByName.Should().Be("Teknik Ofis");
        result.Value.Comments.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task GetDailyPlanDetail_returns_progress_and_rejection_comments_only()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var plan = DailyPlan.CreateFromPlan(
            seed.Project.Id,
            seed.Region.Id,
            seed.Location.Id,
            seed.WorkItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            seed.TechOfficeId,
            seed.HeadOfMasterId,
            10,
            2,
            WorkUnit.M2).Value;
        plan.StartWork(seed.HeadOfMasterId, Guid.NewGuid()).IsSuccess.Should().BeTrue();
        plan.SubmitProgress(5, 1, null, "pipe delayed", seed.HeadOfMasterId).IsSuccess.Should().BeTrue();
        plan.Reject(WorkStatus.ApprovedBySiteChief, seed.SiteChiefId, "revise quantity").IsSuccess.Should().BeTrue();
        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);

        var currentUser = new CurrentUserStub(seed.PmId, [Roles.ProjectManager]);
        var identity = new IdentityStub(new Dictionary<Guid, string>
        {
            [seed.PmId] = "Mehmet Kaya",
            [seed.SiteChiefId] = "Ayşe Demir",
            [seed.HeadOfMasterId] = "Mustafa Arslan",
            [seed.TechOfficeId] = "Teknik Ofis"
        });
        var handler = new GetDailyPlanDetailQueryHandler(db, new AccessScopeService(db, currentUser), identity);

        var result = await handler.Handle(new GetDailyPlanDetailQuery(plan.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Comment.Should().Be("pipe delayed");
        result.Value.History.Should().HaveCount(4);
        result.Value.Comments.Should().HaveCount(2);
        result.Value.Comments.Select(comment => comment.Text)
            .Should().Equal("pipe delayed", "revise quantity");
        result.Value.Comments.Select(comment => comment.Kind)
            .Should().Equal(DailyPlanCommentKind.Progress, DailyPlanCommentKind.Rejection);
        result.Value.Comments.Select(comment => comment.ActionByName)
            .Should().Equal("Mustafa Arslan", "Ayşe Demir");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task GetDailyPlanDetail_classifies_project_manager_return_to_submitted_as_rejection()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var plan = DailyPlan.CreateFromPlan(
            seed.Project.Id,
            seed.Region.Id,
            seed.Location.Id,
            seed.WorkItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            seed.TechOfficeId,
            seed.HeadOfMasterId,
            10,
            2,
            WorkUnit.M2).Value;
        plan.StartWork(seed.HeadOfMasterId, Guid.NewGuid()).IsSuccess.Should().BeTrue();
        plan.SubmitProgress(5, 1, null, "field done", seed.HeadOfMasterId).IsSuccess.Should().BeTrue();
        plan.Approve(WorkStatus.ApprovedBySiteChief, Guid.NewGuid(), seed.SiteChiefId).IsSuccess.Should().BeTrue();
        plan.Reject(WorkStatus.ApprovedByPM, seed.PmId, "site chief should review").IsSuccess.Should().BeTrue();
        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);

        var currentUser = new CurrentUserStub(seed.PmId, [Roles.ProjectManager]);
        var identity = new IdentityStub(new Dictionary<Guid, string>
        {
            [seed.PmId] = "Mehmet Kaya",
            [seed.SiteChiefId] = "Ayşe Demir",
            [seed.HeadOfMasterId] = "Mustafa Arslan",
            [seed.TechOfficeId] = "Teknik Ofis"
        });
        var handler = new GetDailyPlanDetailQueryHandler(db, new AccessScopeService(db, currentUser), identity);

        var result = await handler.Handle(new GetDailyPlanDetailQuery(plan.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var rejection = result.Value.Comments.Should()
            .ContainSingle(comment => comment.Text == "site chief should review")
            .Which;
        rejection.Kind.Should().Be(DailyPlanCommentKind.Rejection);
        rejection.ActionByName.Should().Be("Mehmet Kaya");
        rejection.FromStatus.Should().Be(WorkStatus.ApprovedBySiteChief);
        rejection.ToStatus.Should().Be(WorkStatus.Submitted);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task GetDailyPlanDetail_blocks_project_manager_from_other_projects()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var otherPm = Guid.NewGuid();
        var otherProject = Project.Create("P2", "Other Project", otherPm).Value;
        var otherRegion = CrewRegion.Create(otherProject.Id, "R2", "Other Region").Value;
        otherRegion.AssignSiteChief(Guid.NewGuid());
        var otherLocation = Location.Create(otherProject.Id, otherRegion.Id, "Other Location").Value;
        otherLocation.AssignHeadOfMaster(Guid.NewGuid());
        var plan = DailyPlan.CreateFromPlan(
            otherProject.Id,
            otherRegion.Id,
            otherLocation.Id,
            seed.WorkItemType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            seed.TechOfficeId,
            otherLocation.HeadOfMasterUserId!.Value,
            10,
            2,
            WorkUnit.M2).Value;
        db.Projects.Add(otherProject);
        db.CrewRegions.Add(otherRegion);
        db.Locations.Add(otherLocation);
        db.DailyPlans.Add(plan);
        await db.SaveChangesAsync(CancellationToken.None);

        var currentUser = new CurrentUserStub(seed.PmId, [Roles.ProjectManager]);
        var handler = new GetDailyPlanDetailQueryHandler(
            db,
            new AccessScopeService(db, currentUser),
            new IdentityStub(new Dictionary<Guid, string>()));

        var result = await handler.Handle(new GetDailyPlanDetailQuery(plan.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("scope_mismatch");
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

    private sealed class IdentityStub : IIdentityService
    {
        private readonly IReadOnlyDictionary<Guid, string> _displayNames;

        public IdentityStub(IReadOnlyDictionary<Guid, string> displayNames)
        {
            _displayNames = displayNames;
        }

        public Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken ct)
        {
            IReadOnlyDictionary<Guid, string> result = _displayNames
                .Where(pair => userIds.Contains(pair.Key))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            return Task.FromResult(result);
        }

        public Task<Result<Guid>> RegisterAsync(string email, string password, string fullName, IReadOnlyList<string> roles, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<UserSummary>> GetUsersAsync(string? role, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<Result<AuthenticatedUser>> ValidateCredentialsAsync(string email, string password, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<IssuedRefreshToken> IssueRefreshTokenAsync(Guid userId, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<Result<(AuthenticatedUser User, IssuedRefreshToken RefreshToken)>> RotateRefreshTokenAsync(string rawToken, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<Result> RevokeRefreshTokenAsync(string rawToken, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<Result> UpdateUserRolesAsync(Guid userId, IReadOnlyList<string> roles, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<Result> SetUserActiveAsync(Guid userId, bool isActive, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<Result> ResetPasswordAsync(Guid userId, string newPassword, CancellationToken ct) =>
            throw new NotSupportedException();
    }

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
