namespace Workplan.Application.Features.Notifications;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    string? Link,
    Guid? DailyPlanId,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);
