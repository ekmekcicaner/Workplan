using Workplan.Domain.Common;
using Workplan.SharedKernel.Common;

namespace Workplan.Domain.Entities;

public class Notification : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? Link { get; private set; }
    public Guid? DailyPlanId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? ReadAtUtc { get; private set; }

    private Notification()
    {
    }

    private Notification(Guid id, Guid userId, string type, string title, string message, string? link, Guid? dailyPlanId)
    {
        Id = id;
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
        Link = link;
        DailyPlanId = dailyPlanId;
    }

    public static Result<Notification> CreateDailyPlanAssigned(Guid userId, Guid dailyPlanId, DateOnly workDate)
    {
        if (userId == Guid.Empty)
            return Result<Notification>.Fail(Error.Validation("Bildirim kullanıcısı boş olamaz."));

        if (dailyPlanId == Guid.Empty)
            return Result<Notification>.Fail(Error.Validation("Günlük plan ID boş olamaz."));

        return new Notification(
            EntityId.New(),
            userId,
            "DailyPlanAssigned",
            "Yeni iş atandı",
            $"{workDate:yyyy-MM-dd} tarihli günlük plan size atandı.",
            $"daily-plans/{dailyPlanId}",
            dailyPlanId);
    }

    public static Result<Notification> CreateDailyPlanRejected(
        Guid userId,
        Guid dailyPlanId,
        DateOnly workDate,
        string rejectedByLabel,
        string reason)
    {
        if (userId == Guid.Empty)
            return Result<Notification>.Fail(Error.Validation("Bildirim kullanıcısı boş olamaz."));

        if (dailyPlanId == Guid.Empty)
            return Result<Notification>.Fail(Error.Validation("Günlük plan ID boş olamaz."));

        if (string.IsNullOrWhiteSpace(rejectedByLabel))
            return Result<Notification>.Fail(Error.Validation("Reddeden rol boş olamaz."));

        if (string.IsNullOrWhiteSpace(reason))
            return Result<Notification>.Fail(Error.Validation("Red gerekçesi boş olamaz."));

        return new Notification(
            EntityId.New(),
            userId,
            "DailyPlanRejected",
            "İş reddedildi",
            $"{workDate:yyyy-MM-dd} tarihli günlük plan {rejectedByLabel.Trim()} tarafından reddedildi. Tekrar kontrol edin.",
            $"daily-plans/{dailyPlanId}",
            dailyPlanId);
    }

    public Result MarkAsRead(Guid actorUserId)
    {
        if (actorUserId != UserId)
            return Result.Fail(Error.ScopeMismatch("Bu bildirimi okundu işaretleme yetkiniz yok."));

        ReadAtUtc ??= DateTime.UtcNow;
        return Result.Ok();
    }
}
