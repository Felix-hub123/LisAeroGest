using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class FlightDetailsPage : ContentPage
    {
        public FlightDetailsPage(FlightDetailsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is FlightDetailsViewModel viewModel
                && viewModel.Flight == null
                && viewModel.FlightId > 0)
            {
                await viewModel.LoadAsync();
            }
        }
    }
}
