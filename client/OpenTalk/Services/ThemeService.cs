using Avalonia;
using Avalonia.Styling;

namespace OpenTalk.Services;

public enum AppTheme
{
    System = 0,
    Light = 1,
    Dark = 2,
}

public static class ThemeService
{
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.System;

    public static void Apply(AppTheme theme)
    {
        CurrentTheme = theme;

        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        app.RequestedThemeVariant = theme switch
        {
            AppTheme.Light => ThemeVariant.Light,
            AppTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }
}
