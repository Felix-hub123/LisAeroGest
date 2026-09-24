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

            System.Diagnostics.Debug.WriteLine(
          "[FlightBoardPage] CONSTRUTOR EXECUTADO");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            System.Diagnostics.Debug.WriteLine(
                "[FlightBoardPage] ONAPPEARING EXECUTADO");

            if (BindingContext is FlightBoardViewModel vm)
                await vm.LoadDeparturesCommand.ExecuteAsync(null);
        }

      
    }
}
