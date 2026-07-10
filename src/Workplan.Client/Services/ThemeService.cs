using Microsoft.JSInterop;

namespace Workplan.Client.Services;

public enum ThemeMode
{
    System,
    Light,
    Dark
}

public class ThemeService(IJSRuntime js)
{
    private const string GetMethod = "workplanTheme.get";
    private const string SetMethod = "workplanTheme.set";
    private const string ApplyMethod = "workplanTheme.apply";

    public ThemeMode Mode { get; private set; } = ThemeMode.System;

    public bool IsDark { get; private set; }

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        var stored = await js.InvokeAsync<string?>(GetMethod);
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
        IsDark = await js.InvokeAsync<bool>(SetMethod, ToStorageValue(mode));
        Changed?.Invoke();
    }

    private async Task ApplyAsync()
    {
        IsDark = await js.InvokeAsync<bool>(ApplyMethod, ToStorageValue(Mode));
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
