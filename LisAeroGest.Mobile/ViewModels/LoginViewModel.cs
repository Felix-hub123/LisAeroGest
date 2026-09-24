using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy)
                return;

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

                var success =
                    await _authService.LoginAsync(
                        Email,
                        Password);

                if (!success)
                {
                    ErrorMessage =
                        "Email ou password incorretos. Tente novamente.";

                    HasError = true;
                    return;
                }

                // Reconfigura o menu da Shell atual (a que já está a ser
                // mostrada) em vez de criar uma segunda instância invisível.
                // Já troca para a TabBar certa (Passageiro/Funcionário).
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