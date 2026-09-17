using LisAeroGest.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LisAeroGest.Mobile
{
    public partial class App : Application
    {
        private readonly AuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        public App(
            AuthService authService,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _authService = authService;
            _serviceProvider = serviceProvider;

            MainPage =
                _serviceProvider.GetRequiredService<AppShell>();
        }

        protected override Window CreateWindow(
            IActivationState? activationState)
        {
            return new Window(MainPage);
        }

        protected override async void OnStart()
        {
            base.OnStart();

            try
            {
                if (MainPage is AppShell shell)
                {
                    await shell.ConfigurarAutenticacaoAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[App] Erro ao iniciar: {ex}");
            }
        }
    }
}