using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;


namespace LisAeroGest.Mobile.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty] private string _firstName = string.Empty;
        [ObservableProperty] private string _lastName = string.Empty;
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private string _confirmPassword = string.Empty;
        [ObservableProperty] private string _documentNumber = string.Empty;
        [ObservableProperty] private bool _isBusy;
        [ObservableProperty] private string _errorMessage = string.Empty;
        [ObservableProperty] private bool _hasError;

        public RegisterViewModel(AuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (IsBusy)
                return;

            // Validações
            if (string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(DocumentNumber))
            {
                ErrorMessage = "Preencha todos os campos obrigatórios.";
                HasError = true;
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "As passwords não coincidem.";
                HasError = true;
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "A password deve ter pelo menos 6 caracteres.";
                HasError = true;
                return;
            }

            try
            {
                IsBusy = true;
                HasError = false;

                var (success, error) = await _authService.RegisterAsync(
                    FirstName, LastName, Email, Password, DocumentNumber);

                if (success)
                {
                    Application.Current!.MainPage = _serviceProvider.GetRequiredService<AppShell>();
                }
                else
                {
                    ErrorMessage = error ?? "Não foi possível criar a conta.";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro inesperado. Tente novamente.";
                HasError = true;
                System.Diagnostics.Debug.WriteLine($"[RegisterViewModel] {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToLoginAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}