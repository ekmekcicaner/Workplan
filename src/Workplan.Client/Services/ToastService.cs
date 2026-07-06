namespace Workplan.Client.Services;

public enum ToastSeverity
{
    Success,
    Error,
    Info,
    Warning
}

public sealed record ToastMessage(Guid Id, string Text, ToastSeverity Severity);

public class ToastService
{
    private readonly List<ToastMessage> _toasts = [];

    public IReadOnlyList<ToastMessage> Toasts => _toasts;

    public event Action? Changed;

    public void Success(string text) => Add(text, ToastSeverity.Success);
    public void Error(string text) => Add(text, ToastSeverity.Error);
    public void Info(string text) => Add(text, ToastSeverity.Info);
    public void Warning(string text) => Add(text, ToastSeverity.Warning);

    public void Remove(Guid id)
    {
        _toasts.RemoveAll(t => t.Id == id);
        Changed?.Invoke();
    }

    private void Add(string text, ToastSeverity severity)
    {
        var toast = new ToastMessage(Guid.NewGuid(), text, severity);
        _toasts.Add(toast);
        Changed?.Invoke();
        _ = RemoveAfterDelayAsync(toast.Id);
    }

    private async Task RemoveAfterDelayAsync(Guid id)
    {
        await Task.Delay(TimeSpan.FromSeconds(4));
        Remove(id);
    }
}
