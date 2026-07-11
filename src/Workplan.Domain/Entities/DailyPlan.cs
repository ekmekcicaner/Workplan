using Workplan.Domain.Common;
using Workplan.Domain.Enums;
using Workplan.Domain.Events;
using Workplan.Domain.ValueObjects;
using Workplan.SharedKernel.Common;

namespace Workplan.Domain.Entities;

public class DailyPlan : AggregateRoot<Guid>
{
    public Guid ProjectId { get; private set; }
    public Guid CrewRegionId { get; private set; }
    public Guid LocationId { get; private set; }
    public Guid WorkItemTypeId { get; private set; } // Seçilen ToW/SToW/SSToW yaprak düğümü

    public DateOnly WorkDate { get; private set; }
    public Guid PlannedById { get; private set; } // Teknik Ofis
    public Guid? AssignedHoMId { get; private set; }
    public Guid? CrewTypeId { get; private set; }

    public decimal PlannedQuantity { get; private set; }
    public decimal PlannedManDay { get; private set; }
    public Unit Unit { get; private set; }

    public decimal? FactQuantity { get; private set; }
    public decimal? FactManDay { get; private set; }
    public decimal? Overtime { get; private set; }
    public string? Comment { get; private set; }

    public WorkStatus Status { get; private set; } = WorkStatus.Draft;

    private readonly List<StatusTransition> _history = new();
    public IReadOnlyCollection<StatusTransition> History => _history.AsReadOnly();

    // EF Core için boş constructor (Base taraftaki Id atamasını ezmemesi için)
    private DailyPlan()
    {
    }

    private void ApplyTransition(WorkStatus toStatus, Guid actionById, string? note = null)
    {
        _history.Add(new StatusTransition(this.Status, toStatus, actionById, note));
        this.Status = toStatus;
    }

    // T-1: Plan Oluşturma (Factory Method)
    public static Result<DailyPlan> CreateFromPlan(
        Guid projectId, Guid regionId, Guid locationId, Guid workItemTypeId,
        DateOnly date, Guid plannedById, Guid assignedHoMId,
        decimal plannedQty, decimal plannedManDay, Unit unit)
    {
        if (plannedQty < 0 || plannedManDay < 0)
            return Result<DailyPlan>.Fail(Error.Validation("Gönderi boş olamaz."));

        if (assignedHoMId == Guid.Empty)
            return Result<DailyPlan>.Fail(Error.Validation("Ustabaşı seçilmelidir."));

        if (unit == Unit.None)
            return Result<DailyPlan>.Fail(Error.Validation("Birim seçilmelidir."));

        var work = new DailyPlan
        {
            Id = EntityId.New(),
            ProjectId = projectId,
            CrewRegionId = regionId,
            LocationId = locationId,
            WorkItemTypeId = workItemTypeId,
            WorkDate = date,
            PlannedById = plannedById,
            AssignedHoMId = assignedHoMId,
            PlannedQuantity = plannedQty,
            PlannedManDay = plannedManDay,
            Unit = unit,
            Status = WorkStatus.Draft
        };

        work.ApplyTransition(WorkStatus.Assigned, plannedById,
            $"Teknik Ofis işi ustabaşına ({assignedHoMId}) atadı.");

        return work;
    }

    // T0: Ustabaşı işi başlatır, crew type seçer.
    public Result StartWork(Guid actorId, Guid crewTypeId)
    {
        if (Status != WorkStatus.Assigned)
            return Result.Fail(Error.Validation("İş başlatılabilir durumda değil."));

        if (AssignedHoMId != actorId)
            return Result.Fail(Error.ScopeMismatch("Bu iş size atanmış değil."));

        if (crewTypeId == Guid.Empty)
            return Result.Fail(Error.Validation("Ekip tipi seçilmelidir."));

        CrewTypeId = crewTypeId;
        ApplyTransition(WorkStatus.InProgress, actorId, "Ustabaşı işi başlattı.");
        return Result.Ok();
    }

    public Result SubmitProgress(decimal? factQty, decimal? factManDay, decimal? overtime, string? comment,
        Guid masterId)
    {
        if (Status != WorkStatus.InProgress)
            return Result.Fail(Error.NotFound("İş şu an yürütülme aşamasında değil."));

        if (AssignedHoMId != masterId)
            return Result.Fail(Error.ScopeMismatch("Bu iş size atanmış değil."));

        if (factQty < 0 || factManDay < 0 || overtime < 0)
            return Result.Fail(Error.Validation("Girdiğiniz değerler negatif olamaz."));

        bool hasAnyActual = (factQty.HasValue && factQty.Value > 0) || (factManDay.HasValue && factManDay.Value > 0);
        bool hasCompleteActuals = factQty.HasValue && factQty.Value > 0
                                  && factManDay.HasValue && factManDay.Value > 0;

        if (hasAnyActual && !hasCompleteActuals)
            return Result.Fail(Error.Validation("İş gerçekleştiyse gerçek miktar ve gerçek adam-gün birlikte girilmelidir."));

        if (!hasAnyActual && string.IsNullOrWhiteSpace(comment))
            return Result.Fail(Error.Validation("O gün hiçbir ilerleme kaydedilmediyse gerekçe yazmalısınız."));

        var trimmedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

        FactQuantity = factQty;
        FactManDay = factManDay;
        Overtime = overtime;
        Comment = trimmedComment;

        ApplyTransition(WorkStatus.Submitted, masterId, trimmedComment);
        return Result.Ok();
    }

    // Site Chief -> Project Manager onay motoru.
    public Result Approve(WorkStatus currentApproverRole, Guid userScopeId, Guid approverUserId)
    {
        if (Status == WorkStatus.Submitted && currentApproverRole == WorkStatus.ApprovedBySiteChief)
        {
            ApplyTransition(WorkStatus.ApprovedBySiteChief, approverUserId, "Site Chief onayladı.");
        }
        else if (Status == WorkStatus.ApprovedBySiteChief && currentApproverRole == WorkStatus.ApprovedByPM)
        {
            ApplyTransition(WorkStatus.ApprovedByPM, approverUserId, "Project Manager son onayı verdi. İş kapatıldı.");

            Raise(new DailyWorkApprovedFullyEvent(Id));
        }
        else
        {
            return Result.Fail(Error.Validation("Bu iş şu anda sizin onay aşamanızda değil."));
        }

        return Result.Ok();
    }

    public Result Reject(WorkStatus rejecterRole, Guid approverUserId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Fail(Error.Validation("Red gerekçesi boş olamaz."));

        var targetStatus = (rejecterRole, Status) switch
        {
            (WorkStatus.ApprovedBySiteChief, WorkStatus.Submitted) => WorkStatus.InProgress,
            (WorkStatus.ApprovedByPM, WorkStatus.ApprovedBySiteChief) => WorkStatus.Submitted,
            _ => (WorkStatus?)null
        };

        if (targetStatus is null)
            return Result.Fail(Error.Validation("Bu iş şu anda sizin red aşamanızda değil."));

        string trimmedReason = reason.Trim();

        ApplyTransition(targetStatus.Value, approverUserId, trimmedReason);

        return Result.Ok();
    }
}
