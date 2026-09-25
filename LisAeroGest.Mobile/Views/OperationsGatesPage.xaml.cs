using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class OperationsGatesPage :
    ContentPage,
    IQueryAttributable
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

    // =========================================================
    // RECEBER O VOO DA PÁGINA DE OPERAÇÃO
    // =========================================================

    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (query.TryGetValue(
                "flightId",
                out var value) &&
            int.TryParse(
                value?.ToString(),
                out var flightId))
        {
            _viewModel.FlightId = flightId;

            // Obriga a carregar novamente para abrir
            // diretamente o voo recebido.
            _hasLoaded = false;
        }
    }

    // =========================================================
    // ABRIR PÁGINA
    // =========================================================

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_hasLoaded)
        {
            _hasLoaded = true;

            await _viewModel
                .LoadCommand
                .ExecuteAsync(null);
        }
    }

    // =========================================================
    // PULL TO REFRESH
    // =========================================================

    private async void OnRefreshing(
        object sender,
        EventArgs e)
    {
        try
        {
            await _viewModel
                .LoadCommand
                .ExecuteAsync(null);
        }
        finally
        {
            Refresh.IsRefreshing = false;
        }
    }
}