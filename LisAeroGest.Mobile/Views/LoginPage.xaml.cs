using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }

        private async void OnPortalClicked(object sender, EventArgs e)
        {
            await Launcher.Default.OpenAsync("https://lisaerogest.onrender.com/Account/Register");
        }
    }
}
