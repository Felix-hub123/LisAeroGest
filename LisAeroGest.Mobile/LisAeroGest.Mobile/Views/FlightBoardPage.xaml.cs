using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class FlightBoardPage : ContentPage
    {
        public FlightBoardPage(FlightBoardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is FlightBoardViewModel vm)
            {
                await vm.LoadDeparturesCommand.ExecuteAsync(null);
            }
        }

        private async void OnFlightTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is not FlightDto flight)
                return;

            if (sender is VisualElement element)
            {
                await element.FadeTo(0.45, 70);
                await element.FadeTo(1.0, 70);
            }

            if (BindingContext is FlightBoardViewModel vm
                && vm.SelectFlightCommand.CanExecute(flight))
            {
                await vm.SelectFlightCommand.ExecuteAsync(flight);
            }
        }
    }
}