using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.ViewModels;
using LisAeroGest.Mobile.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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

            // ── Handler de autenticação (anexa o token JWT aos pedidos) ───
            builder.Services.AddTransient<AuthTokenHandler>();

            // ── HttpClient & Serviços ────────────────────────────────────
            builder.Services.AddHttpClient<ApiService>(client =>
            {
                client.BaseAddress = new Uri("https://lisaerogest.onrender.com/");
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
                client.BaseAddress = new Uri("https://lisaerogest.onrender.com/");
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

            // ── ViewModels ───────────────────────────────────────────────
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<FlightBoardViewModel>();
            builder.Services.AddTransient<TicketsViewModel>();

            // ── Views / Páginas ──────────────────────────────────────────
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<FlightBoardPage>();
            builder.Services.AddTransient<TicketsPage>();
            builder.Services.AddTransient<TicketsViewModel>();
            builder.Services.AddTransient<TicketsPage>();
            builder.Services.AddTransient<CheckInViewModel>();
            builder.Services.AddTransient<CheckInPage>();

            return builder.Build();
        }
    }
}