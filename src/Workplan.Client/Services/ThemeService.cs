using Microsoft.JSInterop;

namespace Workplan.Client.Services;

public enum ThemeMode
{
    System,
    Light,
    Dark
}

public class ThemeService
{
    private const string GetMethod = "workplanTheme.get";
    private const string SetMethod = "workplanTheme.set";
    private const string ApplyMethod = "workplanTheme.apply";

    private readonly IJSRuntime _js;

    public ThemeMode Mode { get; private set; } = ThemeMode.System;

    public bool IsDark { get; private set; }

    public event Action? Changed;

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        var stored = await _js.InvokeAsync<string?>(GetMethod);
        Mode = stored switch
        {
            "dark" => ThemeMode.Dark,
            "light" => ThemeMode.Light,
            _ => ThemeMode.System
        };

        await ApplyAsync();
    }

    public async Task ToggleAsync()
    {
        await SetModeAsync(IsDark ? ThemeMode.Light : ThemeMode.Dark);
    }

    public async Task SetModeAsync(ThemeMode mode)
    {
        Mode = mode;
        IsDark = await _js.InvokeAsync<bool>(SetMethod, ToStorageValue(mode));
        Changed?.Invoke();
    }

    private async Task ApplyAsync()
    {
        IsDark = await _js.InvokeAsync<bool>(ApplyMethod, ToStorageValue(Mode));
        Changed?.Invoke();
    }

    private static string ToStorageValue(ThemeMode mode) =>
        mode switch
        {
            ThemeMode.Dark => "dark",
            ThemeMode.Light => "light",
            _ => "system"
        };
}
