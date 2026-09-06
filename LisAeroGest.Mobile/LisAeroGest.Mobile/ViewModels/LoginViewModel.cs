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

        /// <summary>
        /// Email introduzido pelo utilizador.
        /// </summary>
        [ObservableProperty]
        private string _email = string.Empty;

        /// <summary>
        /// Password introduzida pelo utilizador.
        /// </summary>
        [ObservableProperty]
        private string _password = string.Empty;

        /// <summary>
        /// Indica se está a decorrer uma operação de login.
        /// </summary>
        [ObservableProperty]
        private bool _isBusy;

        /// <summary>
        /// Mensagem de erro a mostrar ao utilizador.
        /// </summary>
        [ObservableProperty]
        private string _errorMessage = string.Empty;

        /// <summary>
        /// Indica se existe uma mensagem de erro para mostrar.
        /// </summary>
        [ObservableProperty]
        private bool _hasError;

        /// <summary>
        /// Inicializa o LoginViewModel com o AuthService injectado.
        /// </summary>
        public LoginViewModel(AuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Executa o processo de login com as credenciais introduzidas.
        /// Redireciona para o painel de voos em caso de sucesso.
        /// </summary>
        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Por favor, introduza o email e a password.";
                HasError = true;
                return;
            }

            try
            {
                IsBusy = true;
                HasError = false;

                var success = await _authService.LoginAsync(Email, Password);

                if (success)
                    Application.Current!.MainPage = _serviceProvider.GetRequiredService<AppShell>();
                else
                {
                    ErrorMessage = "Email ou password incorretos. Tente novamente.";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro de ligação. Verifique a sua internet.";
                HasError = true;
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Erro: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}