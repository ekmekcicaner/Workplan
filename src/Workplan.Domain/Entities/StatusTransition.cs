using Workplan.Domain.Common;
using Workplan.Domain.Enums;

namespace Workplan.Domain.Entities;

public class StatusTransition
{
    public Guid Id { get; private set; }
    public WorkStatus FromStatus { get; private set; }
    public WorkStatus ToStatus { get; private set; }
    public Guid ActionById { get; private set; } // İşlemi yapan kişi (Mühendis, Master, PM vb.)
    public DateTime TransitionedAt { get; private set; } = DateTime.UtcNow;
    public string? Note { get; private set; } // Red gerekçesi veya özel notlar

    private StatusTransition()
    {
    } // EF Core için

    public StatusTransition(WorkStatus fromStatus, WorkStatus toStatus, Guid actionById, string? note = null)
    {
        Id = EntityId.New();
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ActionById = actionById;
        Note = note;
    }
}
