using LisAeroGest.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LisAeroGest.Mobile.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public LoginPage(
            LoginViewModel viewModel,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();

            BindingContext = viewModel;
            _serviceProvider = serviceProvider;
        }

        private void OnRegisterClicked(object sender, EventArgs e)
        {
            Application.Current!.MainPage =
                _serviceProvider.GetRequiredService<RegisterPage>();
        }
    }
}