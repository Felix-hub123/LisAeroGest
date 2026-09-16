using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.ViewModels;
using LisAeroGest.Mobile.Views;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace LisAeroGest.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // ── Handler de autenticação ────────────────────────────────────
            builder.Services.AddTransient<AuthTokenHandler>();

            // ── HttpClient e serviços ──────────────────────────────────────
            builder.Services.AddHttpClient<ApiService>(client =>
            {
                client.BaseAddress = new Uri(
                    "https://lisaerogest.onrender.com/");
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .AddHttpMessageHandler<AuthTokenHandler>()
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

#if DEBUG
                handler.ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => true;
#endif

                return handler;
            });

            builder.Services.AddHttpClient<AuthService>(client =>
            {
                client.BaseAddress = new Uri(
                    "https://lisaerogest.onrender.com/");
                client.Timeout = TimeSpan.FromSeconds(15);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

#if DEBUG
                handler.ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => true;
#endif

                return handler;
            });

            // ── ViewModels existentes ───────────────────────────────────
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<FlightBoardViewModel>();
            builder.Services.AddTransient<FlightDetailsViewModel>();
            builder.Services.AddTransient<TicketsViewModel>();
            builder.Services.AddTransient<CheckInViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<SelectSeatViewModel>();



            // ── Views existentes ────────────────────────────────────────
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<FlightBoardPage>();
            builder.Services.AddTransient<FlightDetailsPage>();
            builder.Services.AddTransient<TicketsPage>();
            builder.Services.AddTransient<CheckInPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<SelectSeatPage>();



            return builder.Build();
        }
    }
}
