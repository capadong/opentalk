using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTalk.Services;

namespace OpenTalk.ViewModels;

public partial class SettingsWindowViewModel : ViewModelBase
{
    public SettingsWindowViewModel()
    {
        RefreshOptions();
        ApplySettingsCommand = new RelayCommand(ApplySettings);
    }

    public ObservableCollection<ThemeOption> AvailableThemes { get; } = [];
    public ObservableCollection<LanguageOption> AvailableLanguages { get; } = [];

    public string ThemeLabel => LocalizationService.Instance.Format("Settings.CurrentSelection", GetThemeLabel(SelectedTheme));
    public string LanguageLabel => LocalizationService.Instance.Format("Settings.CurrentSelection", GetLanguageLabel(SelectedLanguage));

    [ObservableProperty]
    private ThemeOption? selectedThemeOption;

    [ObservableProperty]
    private LanguageOption? selectedLanguageOption;

    public AppTheme SelectedTheme => SelectedThemeOption?.Value ?? ThemeService.CurrentTheme;
    public AppLanguage SelectedLanguage => SelectedLanguageOption?.Value ?? LocalizationService.Instance.CurrentLanguage;

    public IRelayCommand ApplySettingsCommand { get; }

    private void ApplySettings()
    {
        ThemeService.Apply(SelectedTheme);
        LocalizationService.Instance.Apply(SelectedLanguage);
        RefreshOptions();
        OnPropertyChanged(nameof(ThemeLabel));
        OnPropertyChanged(nameof(LanguageLabel));
    }

    partial void OnSelectedThemeOptionChanged(ThemeOption? value)
    {
        OnPropertyChanged(nameof(ThemeLabel));
    }

    partial void OnSelectedLanguageOptionChanged(LanguageOption? value)
    {
        OnPropertyChanged(nameof(LanguageLabel));
    }

    private void RefreshOptions()
    {
        var currentTheme = SelectedTheme;
        var currentLanguage = SelectedLanguage;

        AvailableThemes.Clear();
        foreach (var theme in Enum.GetValues<AppTheme>())
        {
            AvailableThemes.Add(new ThemeOption(theme, GetThemeLabel(theme)));
        }
        SelectedThemeOption = AvailableThemes.FirstOrDefault(option => option.Value == currentTheme);

        AvailableLanguages.Clear();
        foreach (var language in Enum.GetValues<AppLanguage>())
        {
            AvailableLanguages.Add(new LanguageOption(language, GetLanguageLabel(language)));
        }
        SelectedLanguageOption = AvailableLanguages.FirstOrDefault(option => option.Value == currentLanguage);
    }

    private static string GetThemeLabel(AppTheme theme)
    {
        return theme switch
        {
            AppTheme.System => LocalizationService.Instance.GetString("Settings.Theme.System"),
            AppTheme.Light => LocalizationService.Instance.GetString("Settings.Theme.Light"),
            AppTheme.Dark => LocalizationService.Instance.GetString("Settings.Theme.Dark"),
            _ => theme.ToString(),
        };
    }

    private static string GetLanguageLabel(AppLanguage language)
    {
        return language switch
        {
            AppLanguage.System => LocalizationService.Instance.GetString("Settings.Language.System"),
            AppLanguage.ChineseSimplified => LocalizationService.Instance.GetString("Settings.Language.ChineseSimplified"),
            AppLanguage.English => LocalizationService.Instance.GetString("Settings.Language.English"),
            _ => language.ToString(),
        };
    }

    public sealed record ThemeOption(AppTheme Value, string Label);

    public sealed record LanguageOption(AppLanguage Value, string Label);
}
