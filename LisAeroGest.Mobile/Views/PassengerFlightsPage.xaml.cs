using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class PassengerFlightsPage : ContentPage
{
    private readonly FlightBoardViewModel _viewModel;

    public PassengerFlightsPage(FlightBoardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDeparturesCommand.ExecuteAsync(null);
    }
}
