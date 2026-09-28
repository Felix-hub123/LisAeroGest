using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class MyTripPage : ContentPage
    {
        private readonly MyTripViewModel _viewModel;


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public MyTripPage(
            MyTripViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;
        }


        // =========================================================
        // CARREGAR VIAGEM
        // =========================================================

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel
                .LoadCommand
                .ExecuteAsync(null);
        }


        // =========================================================
        // CARTEIRA
        // =========================================================

        private async void OnVerBilhetesClicked(
            object sender,
            EventArgs e)
        {
            await Shell.Current.GoToAsync(
                "//TicketsPage");
        }


        // =========================================================
        // CHECK-IN
        // =========================================================

        private async void OnCheckInClicked(
            object sender,
            EventArgs e)
        {
            if (_viewModel.CurrentTrip == null)
                return;


            await Shell.Current.GoToAsync(
                nameof(CheckInPage),
                new Dictionary<string, object>
                {
                    ["ticketId"] =
                        _viewModel.CurrentTrip.Id
                });
        }
    }
}