namespace Workplan.Client.Services;

public sealed record ConfirmRequest(string Title, string Message, bool Danger, string ConfirmText, string CancelText);

public class ConfirmService
{
    private TaskCompletionSource<bool>? _pending;

    public ConfirmRequest? Current { get; private set; }

    public event Action? Changed;

    public Task<bool> ConfirmAsync(
        string title, string message, bool danger = false, string confirmText = "Onayla", string cancelText = "Vazgeç")
    {
        _pending?.TrySetResult(false);

        Current = new ConfirmRequest(title, message, danger, confirmText, cancelText);
        _pending = new TaskCompletionSource<bool>();
        Changed?.Invoke();

        return _pending.Task;
    }

    public void Resolve(bool confirmed)
    {
        Current = null;
        _pending?.TrySetResult(confirmed);
        _pending = null;
        Changed?.Invoke();
    }
}
