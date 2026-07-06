namespace Workplan.Client.Models;

public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    string? Link,
    Guid? DailyPlanId,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc);
