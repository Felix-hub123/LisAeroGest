using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _documentNumber = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;


        public RegisterViewModel(
            AuthService authService)
        {
            _authService = authService;
        }


        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (IsBusy)
                return;


            HasError = false;
            ErrorMessage = string.Empty;


            // ==========================================
            // VALIDAÇÃO DOS CAMPOS
            // ==========================================

            if (string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(DocumentNumber) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage =
                    "Preencha todos os campos obrigatórios.";

                HasError = true;

                return;
            }


            if (!Email.Contains("@"))
            {
                ErrorMessage =
                    "Introduza um email válido.";

                HasError = true;

                return;
            }


            if (Password.Length < 6)
            {
                ErrorMessage =
                    "A password deve ter pelo menos 6 caracteres.";

                HasError = true;

                return;
            }


            if (Password != ConfirmPassword)
            {
                ErrorMessage =
                    "As passwords não coincidem.";

                HasError = true;

                return;
            }


            try
            {
                IsBusy = true;


                // ==========================================
                // REGISTO NA API
                // ==========================================

                var result =
                    await _authService.RegisterAsync(
                        FirstName.Trim(),
                        LastName.Trim(),
                        Email.Trim(),
                        Password,
                        DocumentNumber.Trim());


                if (!result.Success)
                {
                    ErrorMessage =
                        result.ErrorMessage
                        ?? "Não foi possível criar a conta.";

                    HasError = true;

                    return;
                }


                // ==========================================
                // CONTA CRIADA COM SUCESSO
                // ==========================================

                await Shell.Current.DisplayAlert(
                    "Conta criada",
                    "O seu perfil de passageiro foi criado com sucesso.",
                    "Continuar");


                // O AuthService já guardou o token JWT.
                // Reconfiguramos agora a Shell para mostrar
                // a área de Passageiro.

                if (Shell.Current is AppShell shell)
                {
                    await shell.ConfigureAuthenticationAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Ocorreu um erro ao criar a conta. Tente novamente.";

                HasError = true;

                System.Diagnostics.Debug.WriteLine(
                    $"[RegisterViewModel] Erro: {ex}");
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