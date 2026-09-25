using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class OperationsCommunicationsPage :
    ContentPage,
    IQueryAttributable
{
    private readonly OperationsCommunicationsViewModel _viewModel;

    private bool _hasLoaded;

    public OperationsCommunicationsPage(
        OperationsCommunicationsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (query.TryGetValue("flightId", out var value) &&
            int.TryParse(value?.ToString(), out var flightId))
        {
            _viewModel.FlightId = flightId;
            _hasLoaded = false;
        }
    }

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
}