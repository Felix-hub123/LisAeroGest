using LisAeroGest.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace LisAeroGest.Mobile
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AppPreferencesService _preferences;


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public App(
            IServiceProvider serviceProvider,
            AppPreferencesService preferences)
        {
            InitializeComponent();

            _serviceProvider =
                serviceProvider;

            _preferences =
                preferences;


            // =====================================================
            // TEMA
            // =====================================================

            _preferences.ApplyTheme();


            // =====================================================
            // SHELL
            // =====================================================

            MainPage =
                _serviceProvider
                    .GetRequiredService<AppShell>();
        }


        // =========================================================
        // JANELA
        // =========================================================

        protected override Window CreateWindow(
            IActivationState? activationState)
        {
            return new Window(
                MainPage);
        }


        // =========================================================
        // INICIALIZAÇÃO
        // =========================================================

        protected override async void OnStart()
        {
            base.OnStart();

            try
            {
                if (MainPage is AppShell shell)
                {
                    await shell
                        .ConfigureAuthenticationAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[App] Erro ao iniciar: {ex}");
            }
        }
    }
}