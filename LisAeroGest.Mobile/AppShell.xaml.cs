using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;

namespace LisAeroGest.Mobile
{
    public partial class AppShell : Shell
    {
        private readonly AuthService _authService;

        private static bool _sessionExpiredAlreadyShown = false;
        private bool _isChangingMenu = false;

        public AppShell(AuthService authService)
        {
            InitializeComponent();

            _authService = authService;

            // Passenger routes
            Routing.RegisterRoute(
             nameof(CheckInPage),
             new DiRouteFactory<CheckInPage>());
            Routing.RegisterRoute(nameof(FlightDetailsPage), typeof(FlightDetailsPage));
            Routing.RegisterRoute(nameof(SelectSeatPage), typeof(SelectSeatPage));
            Routing.RegisterRoute(nameof(PaymentPage), typeof(PaymentPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
            Routing.RegisterRoute(nameof(FavoritesPage), typeof(FavoritesPage));
            Routing.RegisterRoute(nameof(PassengerFlightsPage), typeof(PassengerFlightsPage));

            // Employee routes
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

            // Listen for HTTP 401 responses
            AuthTokenHandler.UnauthorizedDetected += OnUnauthorizedDetected;
        }

        /// <summary>
        /// Checks authentication and displays the correct menu
        /// according to the authenticated user's role.
        /// </summary>
        public async Task ConfigureAuthenticationAsync()
        {
            var isAuthenticated = await _authService.IsAuthenticatedAsync();

            if (!isAuthenticated)
            {
                await ShowUnauthenticatedMenuAsync();
                return;
            }

            var role = await _authService.GetRoleAsync();

            System.Diagnostics.Debug.WriteLine(
                $"[AppShell] Role: {role}");

            await ShowAuthenticatedMenuAsync(role);
        }

        /// <summary>
        /// Displays only the login area when no user is authenticated.
        /// </summary>
        private async Task ShowUnauthenticatedMenuAsync()
        {
            if (_isChangingMenu)
                return;

            try
            {
                _isChangingMenu = true;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                  
                    LoginItem.IsVisible = true;

                   
                    PassengerTabs.IsVisible = false;
                    EmployeeTabs.IsVisible = false;

                  
                    await Task.Delay(150);

                   
                    var loginShellItem = LoginItem.Parent as ShellItem;

                    if (loginShellItem != null && CurrentItem != loginShellItem)
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

        /// <summary>
        /// Displays the correct authenticated menu according to the user's role.
        /// </summary>
        private async Task ShowAuthenticatedMenuAsync(string? role)
        {
            if (_isChangingMenu)
                return;

            try
            {
                _isChangingMenu = true;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (role is "Employee" or "Admin")
                    {
                        // Make employee tabs visible first
                        EmployeeTabs.IsVisible = true;

                        await Task.Delay(100);

                        if (CurrentItem != EmployeeTabs)
                        {
                            CurrentItem = EmployeeTabs;
                        }

                        // Hide the other areas
                        LoginItem.IsVisible = false;
                        PassengerTabs.IsVisible = false;
                    }
                    else
                    {
                        // Make passenger tabs visible first
                        PassengerTabs.IsVisible = true;

                        await Task.Delay(100);

                        if (CurrentItem != PassengerTabs)
                        {
                            CurrentItem = PassengerTabs;
                        }

                        // Hide the other areas
                        LoginItem.IsVisible = false;
                        EmployeeTabs.IsVisible = false;
                    }
                });
            }
            finally
            {
                _isChangingMenu = false;
            }
        }

        /// <summary>
        /// Called when the API returns HTTP 401.
        /// Clears the current session and returns to the login area.
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

                await MainThread.InvokeOnMainThreadAsync(async () =>
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

        /// <summary>
        /// Logs out the current user and restores the Shell
        /// to its unauthenticated state.
        /// </summary>
        public async Task LogoutAsync()
        {
            _authService.Logout();

            await ShowUnauthenticatedMenuAsync();
        }
    }
}