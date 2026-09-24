using LisAeroGest.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LisAeroGest.Mobile
{
    public partial class App : Application
    {
        private readonly AuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppPreferencesService _preferences;

        public App(
            AuthService authService,
            IServiceProvider serviceProvider,
            AppPreferencesService preferences)
        {
            InitializeComponent();

            _authService = authService;
            _serviceProvider = serviceProvider;
            _preferences = preferences;

            // Escuro/glassmorphism é o tema principal da app — não segue
            // o tema do sistema, para a identidade visual ser consistente
            // em qualquer dispositivo.
            _preferences.ApplyTheme();

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
                    await shell.ConfigureAuthenticationAsync();
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