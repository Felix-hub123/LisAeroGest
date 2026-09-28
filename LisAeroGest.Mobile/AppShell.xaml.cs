using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;
using System.Diagnostics;

namespace LisAeroGest.Mobile
{
    public partial class AppShell : Shell
    {
        private readonly AuthService _authService;
        private readonly ApiService _apiService;

        private static bool _sessionExpiredAlreadyShown;
        private bool _isChangingMenu;


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public AppShell(
            AuthService authService,
            ApiService apiService)
        {
            InitializeComponent();

            _authService =
                authService;

            _apiService =
                apiService;


            RegisterRoutes();


            // =====================================================
            // DETETAR HTTP 401
            // =====================================================

            AuthTokenHandler.UnauthorizedDetected +=
                OnUnauthorizedDetected;
        }


        // =========================================================
        // REGISTAR ROTAS
        // =========================================================

        private static void RegisterRoutes()
        {
            // =====================================================
            // PASSAGEIRO
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
            // FUNCIONÁRIO
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
        }


        // =========================================================
        // CONFIGURAR AUTENTICAÇÃO
        // =========================================================

        public async Task ConfigureAuthenticationAsync()
        {
            var isAuthenticated =
                await _authService
                    .IsAuthenticatedAsync();


            // =====================================================
            // SEM SESSÃO
            // =====================================================

            if (!isAuthenticated)
            {
                await ShowUnauthenticatedMenuAsync();

                return;
            }


            // =====================================================
            // OBTER ROLE
            // =====================================================

            var role =
                await _authService
                    .GetRoleAsync();


            Debug.WriteLine(
                $"[AppShell] Role: {role}");


            // =====================================================
            // VALIDAR ROLE
            // =====================================================

            if (!IsSupportedRole(role))
            {
                Debug.WriteLine(
                    $"[AppShell] Role não suportada: {role}");

                _authService.Logout();

                await ShowUnauthenticatedMenuAsync();

                await MainThread.InvokeOnMainThreadAsync(
                    async () =>
                    {
                        await DisplayAlert(
                            "Acesso indisponível",
                            "Este perfil não possui acesso à aplicação mobile.",
                            "OK");
                    });

                return;
            }


            // =====================================================
            // MOSTRAR MENU
            // =====================================================

            await ShowAuthenticatedMenuAsync(
                role);
        }


        // =========================================================
        // VALIDAR ROLE
        // =========================================================

        private static bool IsSupportedRole(
            string? role)
        {
            return
                string.Equals(
                    role,
                    "Passenger",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    role,
                    "Employee",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase);
        }


        // =========================================================
        // MENU SEM AUTENTICAÇÃO
        // =========================================================

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
                        // Primeiro mostramos o login.
                        LoginItem.IsVisible = true;

                        // Depois escondemos as áreas privadas.
                        PassengerTabs.IsVisible = false;
                        EmployeeTabs.IsVisible = false;

                        // Limpar contador.
                        NotificationsTab.Title =
                            "Alertas";


                        await Task.Delay(100);


                        var loginShellItem =
                            LoginItem.Parent
                            as ShellItem;


                        if (loginShellItem != null &&
                            CurrentItem != loginShellItem)
                        {
                            CurrentItem =
                                loginShellItem;
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

        private async Task ShowAuthenticatedMenuAsync(
            string? role)
        {
            if (_isChangingMenu)
                return;

            try
            {
                _isChangingMenu = true;


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
                    await MainThread.InvokeOnMainThreadAsync(
                        async () =>
                        {
                            EmployeeTabs.IsVisible =
                                true;

                            PassengerTabs.IsVisible =
                                false;


                            await Task.Delay(100);


                            if (CurrentItem !=
                                EmployeeTabs)
                            {
                                CurrentItem =
                                    EmployeeTabs;
                            }


                            LoginItem.IsVisible =
                                false;


                            // O contador pertence apenas
                            // ao passageiro.
                            NotificationsTab.Title =
                                "Alertas";
                        });

                    return;
                }


                // =================================================
                // PASSAGEIRO
                // =================================================

                if (string.Equals(
                        role,
                        "Passenger",
                        StringComparison.OrdinalIgnoreCase))
                {
                    await MainThread.InvokeOnMainThreadAsync(
                        async () =>
                        {
                            PassengerTabs.IsVisible =
                                true;

                            EmployeeTabs.IsVisible =
                                false;


                            await Task.Delay(100);


                            if (CurrentItem !=
                                PassengerTabs)
                            {
                                CurrentItem =
                                    PassengerTabs;
                            }


                            LoginItem.IsVisible =
                                false;
                        });


                    // Não precisamos bloquear a troca de menu
                    // enquanto consultamos o contador.
                    await RefreshNotificationBadgeAsync();

                    return;
                }


                // =================================================
                // ROLE DESCONHECIDA
                // =================================================

                _authService.Logout();

                await ShowUnauthenticatedMenuAsync();
            }
            finally
            {
                _isChangingMenu = false;
            }
        }


        // =========================================================
        // CONTADOR DE NOTIFICAÇÕES
        // =========================================================

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
                    NotificationsTab.Title =
                        "Alertas";

                    return;
                }


                var count =
                    result.Data.Count;


                NotificationsTab.Title =
                    count > 0
                        ? $"Alertas ({count})"
                        : "Alertas";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[AppShell] Erro ao atualizar alertas: {ex.Message}");

                // Uma falha no contador não deve
                // impedir a utilização da aplicação.
                NotificationsTab.Title =
                    "Alertas";
            }
        }


        // =========================================================
        // SESSÃO EXPIRADA
        // =========================================================

        private async Task OnUnauthorizedDetected()
        {
            if (_sessionExpiredAlreadyShown)
                return;


            _sessionExpiredAlreadyShown =
                true;

            try
            {
                // =================================================
                // LIMPAR SESSÃO
                // =================================================

                _authService.Logout();


                // =================================================
                // REGRESSAR AO LOGIN
                // =================================================

                await ShowUnauthenticatedMenuAsync();


                // =================================================
                // INFORMAR UTILIZADOR
                // =================================================

                await MainThread.InvokeOnMainThreadAsync(
                    async () =>
                    {
                        await DisplayAlert(
                            "Sessão expirada",
                            "A sua sessão expirou. " +
                            "Por favor, inicie sessão novamente.",
                            "OK");
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[AppShell] Erro ao tratar sessão expirada: {ex}");
            }
            finally
            {
                _sessionExpiredAlreadyShown =
                    false;
            }
        }


        // =========================================================
        // LOGOUT
        // =========================================================

        public async Task LogoutAsync()
        {
            try
            {
                _authService.Logout();

                NotificationsTab.Title =
                    "Alertas";

                await ShowUnauthenticatedMenuAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[AppShell] Erro no logout: {ex}");
            }
        }
    }
}