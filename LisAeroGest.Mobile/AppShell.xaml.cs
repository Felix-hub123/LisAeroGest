using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;

namespace LisAeroGest.Mobile
{
    public partial class AppShell : Shell
    {
        private readonly AuthService _authService;
        private readonly ApiService _apiService;

        private static bool _sessionExpiredAlreadyShown = false;
        private bool _isChangingMenu = false;

        public AppShell(
            AuthService authService,
            ApiService apiService)
        {
            InitializeComponent();

            _authService = authService;
            _apiService = apiService;

            // =====================================================
            // ROTAS DO PASSAGEIRO
            // =====================================================

            Routing.RegisterRoute(
                nameof(CheckInPage),
                new DiRouteFactory<CheckInPage>());

            Routing.RegisterRoute(
                nameof(FlightDetailsPage),
                typeof(FlightDetailsPage));

            Routing.RegisterRoute(
                nameof(SelectSeatPage),
                typeof(SelectSeatPage));

            Routing.RegisterRoute(
                nameof(PaymentPage),
                typeof(PaymentPage));

            Routing.RegisterRoute(
                nameof(SettingsPage),
                typeof(SettingsPage));

            Routing.RegisterRoute(
                nameof(HistoryPage),
                typeof(HistoryPage));

            Routing.RegisterRoute(
                nameof(FavoritesPage),
                typeof(FavoritesPage));

            Routing.RegisterRoute(
                nameof(PassengerFlightsPage),
                typeof(PassengerFlightsPage));


            // =====================================================
            // ROTAS DO FUNCIONÁRIO
            // =====================================================

            Routing.RegisterRoute(
                nameof(OperationsPassengersPage),
                typeof(OperationsPassengersPage));

            Routing.RegisterRoute(
                nameof(OperationsGatesPage),
                typeof(OperationsGatesPage));

            Routing.RegisterRoute(
                nameof(OperationsCommunicationsPage),
                typeof(OperationsCommunicationsPage));

            Routing.RegisterRoute(
                nameof(OperationsFlightDetailsPage),
                new DiRouteFactory<OperationsFlightDetailsPage>());


            // =====================================================
            // SESSÃO
            // =====================================================

            // Escuta respostas HTTP 401.
            AuthTokenHandler.UnauthorizedDetected +=
                OnUnauthorizedDetected;
        }


        // =========================================================
        // CONFIGURAR AUTENTICAÇÃO
        // =========================================================

        /// <summary>
        /// Verifica a autenticação e apresenta o menu correto
        /// de acordo com o perfil do utilizador.
        /// </summary>
        public async Task ConfigureAuthenticationAsync()
        {
            var isAuthenticated =
                await _authService.IsAuthenticatedAsync();

            if (!isAuthenticated)
            {
                await ShowUnauthenticatedMenuAsync();
                return;
            }

            var role =
                await _authService.GetRoleAsync();

            System.Diagnostics.Debug.WriteLine(
                $"[AppShell] Role: {role}");

            await ShowAuthenticatedMenuAsync(role);
        }


        // =========================================================
        // MENU SEM AUTENTICAÇÃO
        // =========================================================

        /// <summary>
        /// Apresenta apenas a área de login quando
        /// não existe utilizador autenticado.
        /// </summary>
        private async Task ShowUnauthenticatedMenuAsync()
        {
            if (_isChangingMenu)
                return;

            try
            {
                _isChangingMenu = true;

                await MainThread.InvokeOnMainThreadAsync(
                    async () =>
                    {
                        LoginItem.IsVisible = true;

                        PassengerTabs.IsVisible = false;
                        EmployeeTabs.IsVisible = false;

                        // Limpar contador quando termina a sessão.
                        NotificationsTab.Title = "Alertas";

                        await Task.Delay(150);

                        var loginShellItem =
                            LoginItem.Parent as ShellItem;

                        if (loginShellItem != null &&
                            CurrentItem != loginShellItem)
                        {
                            CurrentItem = loginShellItem;
                        }
                    });
            }
            finally
            {
                _isChangingMenu = false;
            }
        }


        // =========================================================
        // MENU AUTENTICADO
        // =========================================================

        /// <summary>
        /// Apresenta o menu correto de acordo
        /// com o perfil do utilizador.
        /// </summary>
        private async Task ShowAuthenticatedMenuAsync(
            string? role)
        {
            if (_isChangingMenu)
                return;

            try
            {
                _isChangingMenu = true;

                await MainThread.InvokeOnMainThreadAsync(
                    async () =>
                    {
                        // =========================================
                        // FUNCIONÁRIO / ADMIN
                        // =========================================

                        if (role is "Employee" or "Admin")
                        {
                            EmployeeTabs.IsVisible = true;

                            await Task.Delay(100);

                            if (CurrentItem != EmployeeTabs)
                            {
                                CurrentItem = EmployeeTabs;
                            }

                            LoginItem.IsVisible = false;
                            PassengerTabs.IsVisible = false;

                            // O contador é apenas do passageiro.
                            NotificationsTab.Title = "Alertas";
                        }

                        // =========================================
                        // PASSAGEIRO
                        // =========================================

                        else
                        {
                            PassengerTabs.IsVisible = true;

                            await Task.Delay(100);

                            if (CurrentItem != PassengerTabs)
                            {
                                CurrentItem = PassengerTabs;
                            }

                            LoginItem.IsVisible = false;
                            EmployeeTabs.IsVisible = false;

                            // Atualizar contador das notificações.
                            await RefreshNotificationBadgeAsync();
                        }
                    });
            }
            finally
            {
                _isChangingMenu = false;
            }
        }


        // =========================================================
        // CONTADOR DE NOTIFICAÇÕES
        // =========================================================

        /// <summary>
        /// Obtém o número de notificações não lidas
        /// e atualiza o título da tab Alertas.
        /// </summary>
        public async Task RefreshNotificationBadgeAsync()
        {
            try
            {
                var result =
                    await _apiService
                        .GetUnreadNotificationCountAsync();

                if (!result.Success ||
                    result.Data == null)
                {
                    NotificationsTab.Title = "Alertas";
                    return;
                }

                var count = result.Data.Count;

                NotificationsTab.Title =
                    count > 0
                        ? $"Alertas ({count})"
                        : "Alertas";
            }
            catch
            {
                // O contador não deve impedir
                // o funcionamento da aplicação.
                NotificationsTab.Title = "Alertas";
            }
        }


        // =========================================================
        // SESSÃO EXPIRADA
        // =========================================================

        /// <summary>
        /// Executado quando a API devolve HTTP 401.
        /// Limpa a sessão e regressa ao login.
        /// </summary>
        private async Task OnUnauthorizedDetected()
        {
            if (_sessionExpiredAlreadyShown)
                return;

            _sessionExpiredAlreadyShown = true;

            try
            {
                _authService.Logout();

                await ShowUnauthenticatedMenuAsync();

                await MainThread.InvokeOnMainThreadAsync(
                    async () =>
                    {
                        await DisplayAlert(
                            "Sessão expirada",
                            "A sua sessão expirou. Por favor, inicie sessão novamente.",
                            "OK");
                    });
            }
            finally
            {
                _sessionExpiredAlreadyShown = false;
            }
        }


        // =========================================================
        // LOGOUT
        // =========================================================

        /// <summary>
        /// Termina a sessão e restaura o Shell
        /// para o estado sem autenticação.
        /// </summary>
        public async Task LogoutAsync()
        {
            _authService.Logout();

            NotificationsTab.Title = "Alertas";

            await ShowUnauthenticatedMenuAsync();
        }
    }
}