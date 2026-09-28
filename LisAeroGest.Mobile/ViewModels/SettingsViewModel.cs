using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly AppPreferencesService _preferences;

        private bool _isLoading;


        // =========================================================
        // PREFERÊNCIAS
        // =========================================================

        [ObservableProperty]
        private bool _notificationsEnabled;

        [ObservableProperty]
        private string _themeMode = "system";

        [ObservableProperty]
        private string _themeLabel = "Sistema";


        // =========================================================
        // FEEDBACK
        // =========================================================

        [ObservableProperty]
        private string _successMessage = string.Empty;

        [ObservableProperty]
        private bool _hasSuccess;


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public SettingsViewModel(
            AppPreferencesService preferences)
        {
            _preferences = preferences;
        }


        // =========================================================
        // CARREGAR
        // =========================================================

        public void Load()
        {
            try
            {
                _isLoading = true;

                HasSuccess = false;
                SuccessMessage = string.Empty;


                ThemeMode =
                    _preferences.ThemeMode;

                NotificationsEnabled =
                    _preferences.NotificationsEnabled;


                UpdateThemeLabel();
            }
            finally
            {
                _isLoading = false;
            }
        }


        // =========================================================
        // ALTERAR TEMA
        // =========================================================

        [RelayCommand]
        private void SetTheme(
            string mode)
        {
            _preferences.SetTheme(mode);


            ThemeMode =
                _preferences.ThemeMode;


            UpdateThemeLabel();


            ShowSuccess(
                $"Tema alterado para {ThemeLabel}.");
        }


        // =========================================================
        // NOTIFICAÇÕES
        // =========================================================

        partial void OnNotificationsEnabledChanged(
            bool value)
        {
            if (_isLoading)
                return;


            _preferences.NotificationsEnabled =
                value;


            if (value)
            {
                ShowSuccess(
                    "Alertas ativados.");
            }
            else
            {
                ShowSuccess(
                    "Alertas desativados. A Central de Notificações continua disponível.");
            }
        }


        // =========================================================
        // LABEL DO TEMA
        // =========================================================

        private void UpdateThemeLabel()
        {
            ThemeLabel =
                ThemeMode switch
                {
                    "light" => "Claro",
                    "dark" => "Escuro",
                    _ => "Sistema"
                };
        }


        // =========================================================
        // FEEDBACK
        // =========================================================

        private void ShowSuccess(
            string message)
        {
            SuccessMessage = message;
            HasSuccess = true;
        }
    }
}