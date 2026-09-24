using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class OperationsGatesPage : ContentPage
{
    private readonly OperationsGatesViewModel _viewModel;
    private bool _jaCarregou;

    public OperationsGatesPage(OperationsGatesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Só carrega na primeira vez; o RefreshView trata das atualizações
        if (!_jaCarregou)
        {
            _jaCarregou = true;
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await _viewModel.LoadCommand.ExecuteAsync(null);
        Refresh.IsRefreshing = false;
    }
}
