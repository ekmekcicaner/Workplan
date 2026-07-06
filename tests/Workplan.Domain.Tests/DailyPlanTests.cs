using FluentAssertions;
using Workplan.Domain.Entities;
using Workplan.Domain.Enums;
using Workplan.Domain.Events;
using Workplan.Domain.ValueObjects;
using Xunit;

namespace Workplan.Domain.Tests;

public class DailyPlanTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void CreateFromPlan_creates_assigned_plan_and_initial_history()
    {
        var plannedById = Guid.NewGuid();
        var headOfMasterId = Guid.NewGuid();

        var result = DailyPlan.CreateFromPlan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            plannedById,
            headOfMasterId,
            plannedQty: 12,
            plannedManDay: 3,
            Unit.M2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(WorkStatus.Assigned);
        result.Value.AssignedHoMId.Should().Be(headOfMasterId);
        result.Value.History.Should().ContainSingle()
            .Which.ToStatus.Should().Be(WorkStatus.Assigned);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CreateFromPlan_rejects_missing_assignee_unit_and_negative_values()
    {
        DailyPlan.CreateFromPlan(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid(), Guid.Empty,
                1, 1, Unit.M2)
            .IsFailure.Should().BeTrue();

        DailyPlan.CreateFromPlan(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid(), Guid.NewGuid(),
                1, 1, Unit.None)
            .IsFailure.Should().BeTrue();

        DailyPlan.CreateFromPlan(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid(), Guid.NewGuid(),
                -1, 1, Unit.M2)
            .IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void StartWork_requires_assigned_status_assigned_actor_and_non_empty_crew()
    {
        var assignee = Guid.NewGuid();
        var plan = NewAssignedPlan(assignee);

        plan.StartWork(Guid.NewGuid(), Guid.NewGuid()).Error.Code.Should().Be("scope_mismatch");
        plan.StartWork(assignee, Guid.Empty).IsFailure.Should().BeTrue();

        var result = plan.StartWork(assignee, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        plan.Status.Should().Be(WorkStatus.InProgress);
        plan.StartWork(assignee, Guid.NewGuid()).IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SubmitProgress_enforces_actual_value_rules()
    {
        var assignee = Guid.NewGuid();
        var plan = NewInProgressPlan(assignee);

        plan.SubmitProgress(5, null, null, null, assignee).IsFailure.Should().BeTrue();
        plan.SubmitProgress(null, null, null, null, assignee).IsFailure.Should().BeTrue();
        plan.SubmitProgress(-1, 1, null, null, assignee).IsFailure.Should().BeTrue();
        plan.SubmitProgress(null, null, null, "no work today", Guid.NewGuid())
            .Error.Code.Should().Be("scope_mismatch");

        var result = plan.SubmitProgress(10, 2.5m, 1, " done ", assignee);

        result.IsSuccess.Should().BeTrue();
        plan.Status.Should().Be(WorkStatus.Submitted);
        plan.FactQuantity.Should().Be(10);
        plan.FactManDay.Should().Be(2.5m);
        plan.Comment.Should().Be("done");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Approve_moves_through_site_chief_and_pm_and_raises_domain_event()
    {
        var plan = NewSubmittedPlan(Guid.NewGuid());
        var siteChief = Guid.NewGuid();
        var pm = Guid.NewGuid();

        plan.Approve(WorkStatus.ApprovedBySiteChief, Guid.NewGuid(), siteChief)
            .IsSuccess.Should().BeTrue();
        plan.Status.Should().Be(WorkStatus.ApprovedBySiteChief);

        plan.Approve(WorkStatus.ApprovedByPM, Guid.NewGuid(), pm)
            .IsSuccess.Should().BeTrue();

        plan.Status.Should().Be(WorkStatus.ApprovedByPM);
        plan.DomainEvents.OfType<DailyWorkApprovedFullyEvent>()
            .Should().ContainSingle(e => e.DailyPlanId == plan.Id);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Reject_requires_approval_status_and_reason_then_returns_to_in_progress()
    {
        var assignee = Guid.NewGuid();
        var plan = NewInProgressPlan(assignee);

        plan.Reject(WorkStatus.ApprovedBySiteChief, Guid.NewGuid(), "bad status")
            .IsFailure.Should().BeTrue();

        plan.SubmitProgress(1, 1, null, null, assignee).IsSuccess.Should().BeTrue();
        plan.Reject(WorkStatus.ApprovedBySiteChief, Guid.NewGuid(), " ")
            .IsFailure.Should().BeTrue();

        var result = plan.Reject(WorkStatus.ApprovedBySiteChief, Guid.NewGuid(), "revise");

        result.IsSuccess.Should().BeTrue();
        plan.Status.Should().Be(WorkStatus.InProgress);
        plan.Comment.Should().Contain("revise");
    }

    private static DailyPlan NewAssignedPlan(Guid headOfMasterId) =>
        DailyPlan.CreateFromPlan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            Guid.NewGuid(),
            headOfMasterId,
            1,
            1,
            Unit.M2).Value;

    private static DailyPlan NewInProgressPlan(Guid headOfMasterId)
    {
        var plan = NewAssignedPlan(headOfMasterId);
        plan.StartWork(headOfMasterId, Guid.NewGuid()).IsSuccess.Should().BeTrue();
        return plan;
    }

    private static DailyPlan NewSubmittedPlan(Guid headOfMasterId)
    {
        var plan = NewInProgressPlan(headOfMasterId);
        plan.SubmitProgress(1, 1, null, null, headOfMasterId).IsSuccess.Should().BeTrue();
        return plan;
    }
}
