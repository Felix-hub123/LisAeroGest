using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class SelectSeatPage : ContentPage
    {
        public SelectSeatPage(SelectSeatViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is SelectSeatViewModel viewModel
                && viewModel.FlightId > 0
                && viewModel.Seats.Count == 0)
            {
                await viewModel.LoadSeatsAsync();
            }
        }
    }
}
