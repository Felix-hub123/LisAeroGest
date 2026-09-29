using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        // =========================================================
        // CREDENCIAIS
        // =========================================================

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;


        // =========================================================
        // GUARDAR CREDENCIAIS
        // =========================================================

        [ObservableProperty]
        private bool _rememberCredentials;


        // =========================================================
        // MOSTRAR / OCULTAR PASSWORD
        // =========================================================

        [ObservableProperty]
        private bool _isPasswordHidden = true;

        public string PasswordVisibilityIcon =>
            IsPasswordHidden ? "👁" : "🙈";


        // =========================================================
        // ESTADO
        // =========================================================

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;

            LoadSavedCredentials();
        }


        // =========================================================
        // MOSTRAR / OCULTAR PASSWORD
        // =========================================================

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordHidden = !IsPasswordHidden;

            OnPropertyChanged(nameof(PasswordVisibilityIcon));
        }


        // =========================================================
        // ATIVAR / DESATIVAR GUARDAR CREDENCIAIS
        // =========================================================

        [RelayCommand]
        private void ToggleRememberCredentials()
        {
            RememberCredentials = !RememberCredentials;
        }


        // =========================================================
        // CARREGAR CREDENCIAIS GUARDADAS
        // =========================================================

        private async void LoadSavedCredentials()
        {
            try
            {
                var remember =
                    await SecureStorage.Default.GetAsync(
                        "remember_credentials");

                if (remember != "true")
                {
                    RememberCredentials = false;
                    return;
                }

                var savedEmail =
                    await SecureStorage.Default.GetAsync(
                        "saved_email");

                var savedPassword =
                    await SecureStorage.Default.GetAsync(
                        "saved_password");

                if (!string.IsNullOrWhiteSpace(savedEmail))
                {
                    Email = savedEmail;
                }

                if (!string.IsNullOrWhiteSpace(savedPassword))
                {
                    Password = savedPassword;
                }

                RememberCredentials = true;
            }
            catch (Exception ex)
            {
                RememberCredentials = false;

                System.Diagnostics.Debug.WriteLine(
                    $"[LoginViewModel] Erro ao carregar credenciais: {ex.Message}");
            }
        }


        // =========================================================
        // GUARDAR / APAGAR CREDENCIAIS
        // =========================================================

        private async Task SaveCredentialsAsync()
        {
            try
            {
                if (RememberCredentials)
                {
                    await SecureStorage.Default.SetAsync(
                        "remember_credentials",
                        "true");

                    await SecureStorage.Default.SetAsync(
                        "saved_email",
                        Email.Trim());

                    await SecureStorage.Default.SetAsync(
                        "saved_password",
                        Password);
                }
                else
                {
                    SecureStorage.Default.Remove(
                        "remember_credentials");

                    SecureStorage.Default.Remove(
                        "saved_email");

                    SecureStorage.Default.Remove(
                        "saved_password");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[LoginViewModel] Erro ao guardar credenciais: {ex.Message}");
            }
        }


        // =========================================================
        // LOGIN
        // =========================================================

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy)
                return;


            // ---------------------------------------------------------
            // VALIDAR CAMPOS
            // ---------------------------------------------------------

            if (string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage =
                    "Por favor, introduza o email e a password.";

                HasError = true;

                return;
            }


            try
            {
                IsBusy = true;

                HasError = false;

                ErrorMessage = string.Empty;


                // -----------------------------------------------------
                // AUTENTICAÇÃO
                // -----------------------------------------------------

                var success =
                    await _authService.LoginAsync(
                        Email.Trim(),
                        Password);


                if (!success)
                {
                    ErrorMessage =
                        "Email ou password incorretos. Tente novamente.";

                    HasError = true;

                    return;
                }


                // -----------------------------------------------------
                // GUARDAR CREDENCIAIS
                // -----------------------------------------------------

                await SaveCredentialsAsync();


                // -----------------------------------------------------
                // CONFIGURAR SHELL
                // -----------------------------------------------------

                if (Shell.Current is AppShell shell)
                {
                    await shell.ConfigureAuthenticationAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Erro de ligação. Verifique a sua internet.";

                HasError = true;

                System.Diagnostics.Debug.WriteLine(
                    $"[LoginViewModel] Erro: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}