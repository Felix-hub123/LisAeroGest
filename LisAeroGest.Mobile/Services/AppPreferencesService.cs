using Microsoft.Maui.Storage;

namespace LisAeroGest.Mobile.Services
{
    /// <summary>
    /// Preferências locais da aplicação.
    /// São guardadas no dispositivo e não dependem da API.
    /// </summary>
    public class AppPreferencesService
    {
        private const string ThemeKey = "app_theme";
        private const string NotificationsKey = "notifications_enabled";


        // =========================================================
        // TEMA
        // =========================================================

        public string ThemeMode
        {
            get => Preferences.Get(ThemeKey, "system");

            private set =>
                Preferences.Set(ThemeKey, value);
        }


        // =========================================================
        // ALERTAS
        // =========================================================

        public bool NotificationsEnabled
        {
            get => Preferences.Get(NotificationsKey, true);

            set =>
                Preferences.Set(NotificationsKey, value);
        }


        // =========================================================
        // APLICAR TEMA
        // =========================================================

        public void ApplyTheme()
        {
            if (Application.Current == null)
                return;


            Application.Current.UserAppTheme =
                ThemeMode switch
                {
                    "light" => AppTheme.Light,
                    "dark" => AppTheme.Dark,
                    _ => AppTheme.Unspecified
                };
        }


        // =========================================================
        // ALTERAR TEMA
        // =========================================================

        public void SetTheme(string mode)
        {
            // Aceitar apenas valores conhecidos.
            mode = mode?.ToLowerInvariant() ?? "system";


            if (mode != "light" &&
                mode != "dark" &&
                mode != "system")
            {
                mode = "system";
            }


            ThemeMode = mode;

            ApplyTheme();
        }
    }
}