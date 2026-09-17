using LisAeroGest.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LisAeroGest.Mobile.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public RegisterPage(
            RegisterViewModel viewModel,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();

            BindingContext = viewModel;
            _serviceProvider = serviceProvider;
        }

        private void OnLoginClicked(object sender, EventArgs e)
        {
            Application.Current!.MainPage =
                _serviceProvider.GetRequiredService<LoginPage>();
        }
    }
}