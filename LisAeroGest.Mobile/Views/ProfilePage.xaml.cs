using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _viewModel;


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public ProfilePage(
            ProfileViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;
        }


        // =========================================================
        // CARREGAR PERFIL
        // =========================================================

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel
                .LoadCommand
                .ExecuteAsync(null);
        }


        // =========================================================
        // PREFERÊNCIAS
        // =========================================================

        private async void OnSettingsClicked(
            object sender,
            EventArgs e)
        {
            await Shell.Current.GoToAsync(
                nameof(SettingsPage));
        }


        // =========================================================
        // HISTÓRICO
        // =========================================================

        private async void OnHistoryClicked(
            object sender,
            EventArgs e)
        {
            await Shell.Current.GoToAsync(
                nameof(HistoryPage));
        }


        // =========================================================
        // FAVORITOS
        // =========================================================

        private async void OnFavoritesClicked(
            object sender,
            EventArgs e)
        {
            await Shell.Current.GoToAsync(
                nameof(FavoritesPage));
        }
    }
}