using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workplan.Application.Features.DailyPlans.IntegrationEvents;
using Workplan.Domain.Enums;
using Workplan.Infrastructure;
using Workplan.Infrastructure.Persistence;
using Workplan.Infrastructure.Persistence.Seed;
using Workplan.Integration.Tests.TestInfrastructure;
using Xunit;

namespace Workplan.Integration.Tests;

public class DemoDataSeederIntegrationTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Seed_is_idempotent_and_covers_active_workflow_rejections_notifications_and_outbox()
    {
        if (!DockerTestGuard.Enabled)
            return;

        await using var provider = CreateServiceProvider();

        await SeedAsync(provider);
        await using var verifyScope = provider.CreateAsyncScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var plans = await db.DailyPlans
            .Include(plan => plan.History)
            .AsNoTracking()
            .ToListAsync();
        var notifications = await db.Notifications.AsNoTracking().ToListAsync();
        var outboxMessages = await db.OutboxMessages.AsNoTracking().ToListAsync();

        plans.Should().HaveCount(25);
        plans.Select(plan => plan.Status).Distinct().Should().BeEquivalentTo(
        [
            WorkStatus.Assigned,
            WorkStatus.InProgress,
            WorkStatus.Submitted,
            WorkStatus.ApprovedBySiteChief,
            WorkStatus.ApprovedByPM
        ]);

        var siteChiefRejected = plans.Should().ContainSingle(plan =>
                plan.History.Any(transition =>
                    transition.FromStatus == WorkStatus.Submitted
                    && transition.ToStatus == WorkStatus.InProgress))
            .Which;
        var pmRejected = plans.Should().ContainSingle(plan =>
                plan.History.Any(transition =>
                    transition.FromStatus == WorkStatus.ApprovedBySiteChief
                    && transition.ToStatus == WorkStatus.Submitted))
            .Which;

        notifications.Should().ContainSingle(notification =>
            notification.Type == "DailyPlanRejected"
            && notification.DailyPlanId == siteChiefRejected.Id
            && notification.UserId == siteChiefRejected.AssignedHoMId);

        var pmRejectedRegion = await db.CrewRegions
            .AsNoTracking()
            .SingleAsync(region => region.Id == pmRejected.CrewRegionId);
        notifications.Should().ContainSingle(notification =>
            notification.Type == "DailyPlanRejected"
            && notification.DailyPlanId == pmRejected.Id
            && notification.UserId == pmRejectedRegion.SiteChiefUserId);

        var fullyApprovedCount = plans.Count(plan => plan.Status == WorkStatus.ApprovedByPM);
        outboxMessages.Should().HaveCount(fullyApprovedCount);
        outboxMessages.Should().OnlyContain(message => message.Type == DailyPlanFullyApproved.EventName);

        await SeedAsync(provider);

        (await db.Projects.CountAsync()).Should().Be(1);
        (await db.DailyPlans.CountAsync()).Should().Be(25);
        (await db.OutboxMessages.CountAsync()).Should().Be(fullyApprovedCount);
    }

    private ServiceProvider CreateServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = fixture.ConnectionString,
                ["Outbox:PollIntervalSeconds"] = "2",
                ["Outbox:BatchSize"] = "20",
                ["Outbox:MaxRetryCount"] = "8",
                ["Outbox:BaseRetryDelaySeconds"] = "5",
                ["Outbox:MaxRetryDelaySeconds"] = "300",
                ["IntegrationWebhook:Enabled"] = "false",
                ["IntegrationWebhook:TimeoutSeconds"] = "15",
                ["Jwt:Issuer"] = "Workplan.SeedTests",
                ["Jwt:Audience"] = "Workplan.SeedTests",
                ["Jwt:SigningKey"] = "WORKPLAN_SEED_TEST_SIGNING_KEY_32_CHARS_MIN",
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "7"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }

    private static async Task SeedAsync(IServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        await DemoDataSeeder.SeedAsync(scope.ServiceProvider);
    }
}
