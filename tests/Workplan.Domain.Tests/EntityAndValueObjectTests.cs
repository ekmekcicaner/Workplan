using FluentAssertions;
using Workplan.Domain.Entities;
using Workplan.Domain.ValueObjects;
using Xunit;

namespace Workplan.Domain.Tests;

public class EntityAndValueObjectTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Project_trims_updates_assigns_pm_and_toggles_activation()
    {
        var project = Project.Create(" P01 ", " Project ").Value;

        project.Code.Should().Be("P01");
        project.Name.Should().Be("Project");
        project.AssignPm(Guid.Empty).IsFailure.Should().BeTrue();

        var pm = Guid.NewGuid();
        project.AssignPm(pm).IsSuccess.Should().BeTrue();
        project.PmUserId.Should().Be(pm);

        project.Update(" P02 ", " Updated ").IsSuccess.Should().BeTrue();
        project.Code.Should().Be("P02");
        project.Name.Should().Be("Updated");

        project.Deactivate();
        project.IsActive.Should().BeFalse();
        project.Activate();
        project.IsActive.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void WorkItemType_enforces_three_level_unit_rules()
    {
        WorkItemType.Create("root", null, null, Unit.M2).IsFailure.Should().BeTrue();

        var root = WorkItemType.Create("root").Value;
        var child = WorkItemType.Create("child", root.Id, root.Level).Value;

        WorkItemType.Create("leaf", child.Id, child.Level, Unit.None)
            .IsFailure.Should().BeTrue();
        WorkItemType.Create("too-deep", Guid.NewGuid(), 2, Unit.M2)
            .IsFailure.Should().BeTrue();

        var leaf = WorkItemType.Create(" leaf ", child.Id, child.Level, Unit.M3).Value;
        leaf.Level.Should().Be(2);
        leaf.Unit.Should().Be(Unit.M3);
        leaf.Rename(" renamed ").IsSuccess.Should().BeTrue();
        leaf.Name.Should().Be("renamed");
        leaf.SetUnit(Unit.Ton).IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Notification_can_only_be_marked_read_by_owner()
    {
        var owner = Guid.NewGuid();
        var notification = Notification.CreateDailyPlanAssigned(
            owner,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow)).Value;

        notification.MarkAsRead(Guid.NewGuid()).Error.Code.Should().Be("scope_mismatch");

        notification.MarkAsRead(owner).IsSuccess.Should().BeTrue();
        notification.ReadAtUtc.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Quantity_rejects_negative_values_and_preserves_unit()
    {
        Quantity.Create(-1, Unit.M2).IsFailure.Should().BeTrue();

        var quantity = Quantity.Create(2.5m, Unit.M3).Value;

        quantity.Value.Should().Be(2.5m);
        quantity.Unit.Should().Be(Unit.M3);
    }
}
