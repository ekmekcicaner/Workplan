using Microsoft.JSInterop;

namespace Workplan.Client.Services;

public sealed class ConnectivityService(IJSRuntime js) : IAsyncDisposable
{
    private DotNetObjectReference<ConnectivityService>? _selfReference;
    private bool _initialized;

    public bool IsOnline { get; private set; } = true;
    public event Action? Changed;

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        _selfReference = DotNetObjectReference.Create(this);
        try
        {
            IsOnline = await js.InvokeAsync<bool>("workplanConnectivity.isOnline");
            await js.InvokeVoidAsync("workplanConnectivity.register", _selfReference);
            _initialized = true;
        }
        catch (JSException)
        {
            IsOnline = true;
        }
    }

    [JSInvokable]
    public Task SetOnlineState(bool isOnline)
    {
        if (IsOnline == isOnline)
        {
            return Task.CompletedTask;
        }

        IsOnline = isOnline;
        Changed?.Invoke();
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _selfReference?.Dispose();
        return ValueTask.CompletedTask;
    }
}
