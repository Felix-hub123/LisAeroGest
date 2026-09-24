using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppPreferencesService _preferences;

    [ObservableProperty] private bool notificationsEnabled;
    [ObservableProperty] private string themeMode = "system";
    [ObservableProperty] private string themeLabel = "Sistema";

    public SettingsViewModel(AppPreferencesService preferences)
    {
        _preferences = preferences;
        Load();
    }

    public void Load()
    {
        ThemeMode = _preferences.ThemeMode;
        NotificationsEnabled = _preferences.NotificationsEnabled;
        ThemeLabel = ThemeMode switch
        {
            "light" => "Claro",
            "dark" => "Escuro",
            _ => "Sistema"
        };
    }

    [RelayCommand]
    private void SetTheme(string mode)
    {
        _preferences.SetTheme(mode);
        ThemeMode = mode;
        ThemeLabel = mode switch
        {
            "light" => "Claro",
            "dark" => "Escuro",
            _ => "Sistema"
        };
    }

    partial void OnNotificationsEnabledChanged(bool value)
    {
        _preferences.NotificationsEnabled = value;
    }
}
