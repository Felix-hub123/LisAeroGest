using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly AuthService _authService;


        // =========================================================
        // DADOS
        // =========================================================

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _roleLabel = string.Empty;

        [ObservableProperty]
        private string _accountDescription = string.Empty;


        // =========================================================
        // ESTADO
        // =========================================================

        [ObservableProperty]
        private bool _isBusy;


        // =========================================================
        // PERFIL
        // =========================================================

        [ObservableProperty]
        private bool _isPassenger;

        [ObservableProperty]
        private bool _isEmployee;


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public ProfileViewModel(
            AuthService authService)
        {
            _authService = authService;
        }


        // =========================================================
        // CARREGAR PERFIL
        // =========================================================

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;


                // =================================================
                // EMAIL
                // =================================================

                Email =
                    await _authService
                        .GetEmailAsync()
                    ?? string.Empty;


                // =================================================
                // ROLE
                // =================================================

                var role =
                    await _authService
                        .GetRoleAsync();


                // =================================================
                // FUNCIONÁRIO / ADMIN
                // =================================================

                if (string.Equals(
                        role,
                        "Employee",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    string.Equals(
                        role,
                        "Admin",
                        StringComparison.OrdinalIgnoreCase))
                {
                    IsEmployee = true;
                    IsPassenger = false;

                    RoleLabel =
                        string.Equals(
                            role,
                            "Admin",
                            StringComparison.OrdinalIgnoreCase)
                            ? "Administrador · Operações"
                            : "Funcionário · Operações";

                    AccountDescription =
                        "Acede às ferramentas operacionais e mantém a tua conta segura.";

                    return;
                }


                // =================================================
                // PASSAGEIRO
                // =================================================

                IsPassenger = true;
                IsEmployee = false;

                RoleLabel =
                    "Passageiro";

                AccountDescription =
                    "Consulta as tuas preferências, favoritos e histórico de viagens.";
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // LOGOUT
        // =========================================================

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (IsBusy)
                return;


            // =====================================================
            // CONFIRMAÇÃO
            // =====================================================

            var confirmed =
                await Shell.Current.DisplayAlert(
                    "Terminar sessão",
                    "Tens a certeza de que pretendes terminar a sessão?",
                    "Terminar sessão",
                    "Cancelar");


            if (!confirmed)
                return;


            try
            {
                IsBusy = true;


                // =================================================
                // UTILIZAR O LOGOUT CENTRAL DO SHELL
                // =================================================

                if (Shell.Current is AppShell shell)
                {
                    await shell.LogoutAsync();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}