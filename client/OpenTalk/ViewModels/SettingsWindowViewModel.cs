using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTalk.Services;

namespace OpenTalk.ViewModels;

public partial class SettingsWindowViewModel : ViewModelBase
{
    public SettingsWindowViewModel()
    {
        AvailableThemes = Enum.GetValues<AppTheme>().ToList();
        selectedTheme = ThemeService.CurrentTheme;
        ApplyThemeCommand = new RelayCommand(ApplyTheme);
    }

    public List<AppTheme> AvailableThemes { get; }

    public string ThemeLabel => SelectedTheme switch
    {
        AppTheme.System => "跟随系统",
        AppTheme.Light => "浅色",
        AppTheme.Dark => "深色",
        _ => SelectedTheme.ToString(),
    };

    [ObservableProperty]
    private AppTheme selectedTheme;

    public IRelayCommand ApplyThemeCommand { get; }

    private void ApplyTheme()
    {
        ThemeService.Apply(SelectedTheme);
    }

    partial void OnSelectedThemeChanged(AppTheme value)
    {
        OnPropertyChanged(nameof(ThemeLabel));
    }
}
