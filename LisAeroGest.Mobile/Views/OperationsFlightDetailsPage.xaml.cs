using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class OperationsFlightDetailsPage : ContentPage
{
    private readonly OperationsFlightDetailsViewModel _viewModel;

    public OperationsFlightDetailsPage(
        OperationsFlightDetailsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}