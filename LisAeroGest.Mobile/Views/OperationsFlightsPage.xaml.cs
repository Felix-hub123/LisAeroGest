using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class OperationsFlightsPage : ContentPage
{
    private readonly OperationsFlightsViewModel _viewModel;

    public OperationsFlightsPage(OperationsFlightsViewModel viewModel)
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

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await _viewModel.LoadCommand.ExecuteAsync(null);

        Refresh.IsRefreshing = false;
    }
}