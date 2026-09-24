using Microsoft.Maui.Storage;

namespace LisAeroGest.Mobile.Services;

/// <summary>
/// Preferências locais da aplicação. Não depende da API e funciona no Android e Windows.
/// </summary>
public class AppPreferencesService
{
    private const string ThemeKey = "app_theme";
    private const string NotificationsKey = "notifications_enabled";

    public string ThemeMode
    {
        get => Preferences.Get(ThemeKey, "system");
        set => Preferences.Set(ThemeKey, value);
    }

    public bool NotificationsEnabled
    {
        get => Preferences.Get(NotificationsKey, true);
        set => Preferences.Set(NotificationsKey, value);
    }

    public void ApplyTheme()
    {
        if (Application.Current == null)
            return;

        Application.Current.UserAppTheme = ThemeMode switch
        {
            "light" => AppTheme.Light,
            "dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }

    public void SetTheme(string mode)
    {
        ThemeMode = mode;
        ApplyTheme();
    }
}
