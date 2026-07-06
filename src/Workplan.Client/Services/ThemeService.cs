using Microsoft.JSInterop;

namespace Workplan.Client.Services;

public class ThemeService
{
    private readonly IJSRuntime _js;

    public bool IsDark { get; private set; }

    public event Action? Changed;

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        var stored = await _js.InvokeAsync<string?>("workplanTheme.get");
        IsDark = stored switch
        {
            "dark" => true,
            "light" => false,
            _ => await _js.InvokeAsync<bool>("workplanTheme.prefersDark")
        };

        await ApplyAsync();
    }

    public async Task ToggleAsync()
    {
        IsDark = !IsDark;
        await _js.InvokeVoidAsync("workplanTheme.set", IsDark ? "dark" : "light");
        await ApplyAsync();
    }

    private async Task ApplyAsync()
    {
        await _js.InvokeVoidAsync("workplanTheme.apply", IsDark);
        Changed?.Invoke();
    }
}
