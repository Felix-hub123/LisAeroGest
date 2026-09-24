using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class OperationsGatesPage : ContentPage
{
    private readonly OperationsGatesViewModel _viewModel;
    private bool _hasLoaded;

    public OperationsGatesPage(
        OperationsGatesViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_hasLoaded)
        {
            _hasLoaded = true;

            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
    }

    private async void OnRefreshing(
        object sender,
        EventArgs e)
    {
        await _viewModel.LoadCommand.ExecuteAsync(null);

        Refresh.IsRefreshing = false;
    }
}