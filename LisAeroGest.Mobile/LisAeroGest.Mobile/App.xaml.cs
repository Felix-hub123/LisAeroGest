using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LisAeroGest.Mobile
{
    public partial class App : Application
    {
        private readonly AuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        public App(AuthService authService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _authService = authService;
            _serviceProvider = serviceProvider;

            // Página provisória enquanto verificamos se há sessão guardada
            MainPage = new ContentPage();
        }

        protected override async void OnStart()
        {
            base.OnStart();

            var isAuthenticated = await _authService.IsAuthenticatedAsync();

            MainPage = isAuthenticated
                ? _serviceProvider.GetRequiredService<AppShell>()
                : _serviceProvider.GetRequiredService<LoginPage>();
        }
    }
}