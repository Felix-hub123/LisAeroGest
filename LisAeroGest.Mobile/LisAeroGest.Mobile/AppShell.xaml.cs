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


            Routing.RegisterRoute(nameof(OperationsPassengersPage), typeof(OperationsPassengersPage));
            Routing.RegisterRoute(nameof(OperationsGatesPage), typeof(OperationsGatesPage));
            Routing.RegisterRoute(nameof(OperationsCommunicationsPage), typeof(OperationsCommunicationsPage));
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
            LoginItem.IsVisible = true;
            CurrentItem = LoginItem;
            PassengerTabs.IsVisible = false;
            EmployeeTabs.IsVisible = false;
        }

        private void MostrarMenuAutenticado(string? role)
        {
            if (role is "Employee" or "Admin")
            {
                EmployeeTabs.IsVisible = true;
                CurrentItem = EmployeeTabs;
                LoginItem.IsVisible = false;
                PassengerTabs.IsVisible = false;
            }
            else
            {
                PassengerTabs.IsVisible = true;
                CurrentItem = PassengerTabs;
                LoginItem.IsVisible = false;
                EmployeeTabs.IsVisible = false;
            }
        }

        /// <summary>
        /// Termina a sessão e repõe a Shell para o estado não autenticado.
        /// Chamado por qualquer ViewModel que precise de terminar sessão
        /// (ex.: Perfil, Bilhetes) — um único caminho de logout.
        /// </summary>
        public Task LogoutAsync()
        {
            _authService.Logout();

            MostrarMenuNaoAutenticado();

            return Task.CompletedTask;
        }
    }
}
