using Microsoft.JSInterop;

namespace Workplan.Client.Auth;

public class LocalStorageService(IJSRuntime js)
{
    public ValueTask SetAsync(string key, string value) => js.InvokeVoidAsync("localStorage.setItem", key, value);

    public ValueTask<string?> GetAsync(string key) => js.InvokeAsync<string?>("localStorage.getItem", key);

    public ValueTask RemoveAsync(string key) => js.InvokeVoidAsync("localStorage.removeItem", key);
}
