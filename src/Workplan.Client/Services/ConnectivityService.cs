using Microsoft.JSInterop;

namespace Workplan.Client.Services;

public sealed class ConnectivityService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private DotNetObjectReference<ConnectivityService>? _selfReference;
    private bool _initialized;

    public ConnectivityService(IJSRuntime js)
    {
        _js = js;
    }

    public bool IsOnline { get; private set; } = true;
    public event Action? Changed;

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        _selfReference = DotNetObjectReference.Create(this);
        try
        {
            IsOnline = await _js.InvokeAsync<bool>("workplanConnectivity.isOnline");
            await _js.InvokeVoidAsync("workplanConnectivity.register", _selfReference);
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
