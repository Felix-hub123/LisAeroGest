using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly IServiceProvider _serviceProvider;

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

        public LoginViewModel(
            AuthService authService,
            IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
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

                var shell =
                    _serviceProvider.GetRequiredService<AppShell>();

                await shell.ConfigurarAutenticacaoAsync();

                await shell.GoToAsync("//FlightBoardPage");
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