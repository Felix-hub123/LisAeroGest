using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage(ProfileViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is ProfileViewModel vm)
                await vm.LoadCommand.ExecuteAsync(null);
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync(nameof(SettingsPage));

        private async void OnHistoryClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync(nameof(HistoryPage));

        private async void OnFavoritesClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync(nameof(FavoritesPage));
    }
}
