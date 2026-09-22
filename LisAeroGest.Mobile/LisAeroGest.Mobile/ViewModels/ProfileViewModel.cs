using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _roleLabel = string.Empty;

        public ProfileViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            Email = await _authService.GetEmailAsync() ?? string.Empty;

            var role = await _authService.GetRoleAsync();
            RoleLabel = role is "Employee" or "Admin" ? "Funcionário · Operações" : "Passageiro";
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            // Reutiliza o logout da própria Shell — o mesmo caminho usado
            // pelo botão de Bilhetes, para não haver dois fluxos a divergir.
            if (Shell.Current is AppShell shell)
            {
                await shell.LogoutAsync();
            }
        }
    }
}
