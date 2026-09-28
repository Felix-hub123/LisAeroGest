using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.ViewModels;
using LisAeroGest.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace LisAeroGest.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            System.Text.Encoding.RegisterProvider(
                System.Text.CodePagesEncodingProvider.Instance);

            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont(
                        "OpenSans-Regular.ttf",
                        "OpenSansRegular");

                    fonts.AddFont(
                        "OpenSans-Semibold.ttf",
                        "OpenSansSemibold");
                });


#if DEBUG
            builder.Logging.AddDebug();
#endif


            // =====================================================
            // SERVIÇOS GERAIS
            // =====================================================

            builder.Services.AddTransient<AuthTokenHandler>();

            builder.Services.AddSingleton<AppPreferencesService>();

            builder.Services.AddSingleton<FavoritesService>();


            // =====================================================
            // API
            // =====================================================

            builder.Services
                .AddHttpClient<ApiService>(
                    client =>
                    {
                        client.BaseAddress =
                            new Uri(
                                "https://lisaerogest.onrender.com/");

                        client.Timeout =
                            TimeSpan.FromSeconds(30);
                    })
                .AddHttpMessageHandler<AuthTokenHandler>();


            // =====================================================
            // AUTENTICAÇÃO
            // =====================================================

            builder.Services
                .AddHttpClient<AuthService>(
                    client =>
                    {
                        client.BaseAddress =
                            new Uri(
                                "https://lisaerogest.onrender.com/");

                        client.Timeout =
                            TimeSpan.FromSeconds(15);
                    });


            // =====================================================
            // VIEWMODELS - PASSAGEIRO
            // =====================================================

            builder.Services.AddTransient<LoginViewModel>();

            builder.Services.AddTransient<FlightBoardViewModel>();

            builder.Services.AddTransient<FlightDetailsViewModel>();

            builder.Services.AddTransient<TicketsViewModel>();

            builder.Services.AddTransient<CheckInViewModel>();

            builder.Services.AddTransient<SelectSeatViewModel>();

            builder.Services.AddTransient<PaymentViewModel>();

            builder.Services.AddTransient<ProfileViewModel>();

            builder.Services.AddTransient<HomeViewModel>();

            builder.Services.AddTransient<MyTripViewModel>();

            builder.Services.AddTransient<NotificationsViewModel>();

            builder.Services.AddTransient<SettingsViewModel>();

            builder.Services.AddTransient<HistoryViewModel>();

            builder.Services.AddTransient<FavoritesViewModel>();


            // =====================================================
            // VIEWMODELS - FUNCIONÁRIO
            // =====================================================

            builder.Services.AddTransient<OperationsGatesViewModel>();

            builder.Services.AddTransient<OperationsCommunicationsViewModel>();

            builder.Services.AddTransient<OperationsFlightsViewModel>();

            builder.Services.AddTransient<OperationsFlightDetailsViewModel>();


            // =====================================================
            // SHELL
            // =====================================================

            builder.Services.AddTransient<AppShell>();


            // =====================================================
            // VIEWS - AUTENTICAÇÃO
            // =====================================================

            builder.Services.AddTransient<LoginPage>();


            // =====================================================
            // VIEWS - PASSAGEIRO
            // =====================================================

            builder.Services.AddTransient<HomePage>();

            builder.Services.AddTransient<FlightBoardPage>();

            builder.Services.AddTransient<FlightDetailsPage>();

            builder.Services.AddTransient<PassengerFlightsPage>();

            builder.Services.AddTransient<SelectSeatPage>();

            builder.Services.AddTransient<PaymentPage>();

            builder.Services.AddTransient<TicketsPage>();

            builder.Services.AddTransient<CheckInPage>();

            builder.Services.AddTransient<MyTripPage>();

            builder.Services.AddTransient<NotificationsPage>();

            builder.Services.AddTransient<ProfilePage>();

            builder.Services.AddTransient<SettingsPage>();

            builder.Services.AddTransient<HistoryPage>();

            builder.Services.AddTransient<FavoritesPage>();


            // =====================================================
            // VIEWS - FUNCIONÁRIO
            // =====================================================

            builder.Services.AddTransient<OperationsCenterPage>();

            builder.Services.AddTransient<OperationsPassengersPage>();

            builder.Services.AddTransient<OperationsGatesPage>();

            builder.Services.AddTransient<OperationsCommunicationsPage>();

            builder.Services.AddTransient<OperationsFlightsPage>();

            builder.Services.AddTransient<OperationsFlightDetailsPage>();


            // =====================================================
            // CRIAR APLICAÇÃO
            // =====================================================

            return builder.Build();
        }
    }
}