using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;

namespace LisAeroGest.Mobile
{
    public partial class AppShell : Shell
    {
        private readonly AuthService _authService;

        public AppShell(AuthService authService)
        {
            InitializeComponent();

            _authService = authService;

            Routing.RegisterRoute(
                nameof(CheckInPage),
                typeof(CheckInPage));

            Routing.RegisterRoute(
                nameof(FlightDetailsPage),
                typeof(FlightDetailsPage));

            Routing.RegisterRoute(
                nameof(SelectSeatPage),
                typeof(SelectSeatPage));

            Routing.RegisterRoute(
                nameof(PaymentPage),
                typeof(PaymentPage));
        }

        public async Task ConfigurarAutenticacaoAsync()
        {
            var autenticado =
                await _authService.IsAuthenticatedAsync();

            if (!autenticado)
            {
                MostrarMenuNaoAutenticado();
                return;
            }

            var role =
                await _authService.GetRoleAsync();

            System.Diagnostics.Debug.WriteLine(
                $"[AppShell] Role: {role}");

            MostrarMenuAutenticado(role);
        }

        private void MostrarMenuNaoAutenticado()
        {
            // Login e Registo
            LoginItem.IsVisible = true;
            RegisterItem.IsVisible = true;

            // Menu autenticado
            VoosItem.IsVisible = false;
            BilhetesItem.IsVisible = false;
            LerQrItem.IsVisible = false;

            // Logout
            LogoutButton.IsVisible = false;
        }

        private void MostrarMenuAutenticado(string? role)
        {
            // Login e Registo
            LoginItem.IsVisible = false;
            RegisterItem.IsVisible = false;

            // Voos
            VoosItem.IsVisible = true;

            // Logout
            LogoutButton.IsVisible = true;

            if (role == "Employee")
            {
                // Funcionário
                VoosItem.Title = "Voos";

                BilhetesItem.IsVisible = false;
                LerQrItem.IsVisible = true;
            }
            else
            {
                // Passageiro
                VoosItem.Title = "Partidas";

                BilhetesItem.IsVisible = true;
                LerQrItem.IsVisible = false;
            }
        }

        private async void OnLogoutClicked(
            object sender,
            EventArgs e)
        {
            _authService.Logout();

            MostrarMenuNaoAutenticado();

            await GoToAsync("//LoginPage");
        }
    }
}