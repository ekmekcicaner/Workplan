using FluentAssertions;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Workplan.Application.Common;
using Workplan.Application.Features.DailyPlans.Queries.GetDailyTrackingPlans;
using Workplan.Application.Interfaces;
using Workplan.Domain.Entities;
using Workplan.Domain.Enums;
using Workplan.Infrastructure.Persistence;
using Workplan.SharedKernel.Common;
using Xunit;
using WorkUnit = Workplan.Domain.ValueObjects.Unit;
using Roles = Workplan.SharedKernel.Auth.Roles;

namespace Workplan.Application.Tests;

public class DailyTrackingQueryHandlerTests
{
    [Fact]
    [Trait("Category", "Application")]
    public async Task Admin_sees_all_work_for_selected_day_with_responsible_names()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var handler = CreateHandler(db, seed.AdminId, [Roles.SystemAdmin], seed);

        var result = await handler.Handle(EmptyQuery(seed.Today), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(item => item.Id == seed.PlanA.Id && item.AssignedHoMName == "HoM A");
        result.Value.Should().Contain(item => item.Id == seed.PlanB.Id && item.SiteChiefName == "Chief B" && item.ProjectManagerName == "PM B");
        result.Value.Should().OnlyContain(item => item.WorkDate == seed.Today);
    }

    [Theory]
    [InlineData(Roles.ProjectManager, "pm")]
    [InlineData(Roles.SiteChief, "chief")]
    [InlineData(Roles.HeadOfMaster, "hom")]
    [Trait("Category", "Application")]
    public async Task Scoped_roles_see_only_their_own_work(string role, string userKind)
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var userId = userKind switch
        {
            "pm" => seed.PmA,
            "chief" => seed.SiteChiefA,
            _ => seed.HomA
        };
        var handler = CreateHandler(db, userId, [role], seed);

        var result = await handler.Handle(EmptyQuery(seed.Today), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.Id.Should().Be(seed.PlanA.Id);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Multiple_filters_narrow_results_together()
    {
        await using var db = CreateDbContext();
        var seed = await SeedAsync(db);
        var handler = CreateHandler(db, seed.AdminId, [Roles.SystemAdmin], seed);

        var result = await handler.Handle(
            new GetDailyTrackingPlansQuery(
                seed.Today,
                [seed.ProjectA.Id],
                [seed.RegionA.Id],
                [seed.LocationA.Id],
                [seed.HomA],
                [seed.SiteChiefA],
                [seed.PmA],
                [WorkStatus.InProgress]),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.Id.Should().Be(seed.PlanA.Id);
    }

    private static GetDailyTrackingPlansQuery EmptyQuery(DateOnly workDate) =>
        new(workDate, [], [], [], [], [], [], []);

    private static GetDailyTrackingPlansQueryHandler CreateHandler(
        AppDbContext db,
        Guid currentUserId,
        IReadOnlyList<string> roles,
        SeedData seed)
    {
        var identity = new IdentityStub(new Dictionary<Guid, string>
        {
            [seed.PmA] = "PM A",
            [seed.PmB] = "PM B",
            [seed.SiteChiefA] = "Chief A",
            [seed.SiteChiefB] = "Chief B",
            [seed.HomA] = "HoM A",
            [seed.HomB] = "HoM B"
        });

        return new GetDailyTrackingPlansQueryHandler(
            db,
            new AccessScopeService(db, new CurrentUserStub(currentUserId, roles)),
            identity);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<SeedData> SeedAsync(AppDbContext db)
    {
        var today = new DateOnly(2026, 7, 7);
        var admin = Guid.NewGuid();
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
        var workItemType = WorkItemType.Create("Leaf", Guid.NewGuid(), 1, WorkUnit.M2).Value;

        var planA = DailyPlan.CreateFromPlan(
            projectA.Id, regionA.Id, locationA.Id, workItemType.Id, today, techA, homA, 10, 2, WorkUnit.M2).Value;
        planA.StartWork(homA, Guid.NewGuid()).IsSuccess.Should().BeTrue();
        var planB = DailyPlan.CreateFromPlan(
            projectB.Id, regionB.Id, locationB.Id, workItemType.Id, today, techB, homB, 8, 1, WorkUnit.M2).Value;
        var yesterdayPlan = DailyPlan.CreateFromPlan(
            projectA.Id, regionA.Id, locationA.Id, workItemType.Id, today.AddDays(-1), techA, homA, 3, 1, WorkUnit.M2).Value;

        db.Projects.AddRange(projectA, projectB);
        db.CrewRegions.AddRange(regionA, regionB);
        db.Locations.AddRange(locationA, locationB);
        db.WorkItemTypes.Add(workItemType);
        db.DailyPlans.AddRange(planA, planB, yesterdayPlan);
        await db.SaveChangesAsync(CancellationToken.None);

        return new SeedData(
            admin,
            pmA,
            pmB,
            siteChiefA,
            siteChiefB,
            homA,
            homB,
            today,
            projectA,
            regionA,
            locationA,
            planA,
            planB);
    }

    private sealed record CurrentUserStub(Guid? UserId, IReadOnlyList<string> Roles) : ICurrentUserService;

    private sealed record SeedData(
        Guid AdminId,
        Guid PmA,
        Guid PmB,
        Guid SiteChiefA,
        Guid SiteChiefB,
        Guid HomA,
        Guid HomB,
        DateOnly Today,
        Project ProjectA,
        CrewRegion RegionA,
        Location LocationA,
        DailyPlan PlanA,
        DailyPlan PlanB);

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
