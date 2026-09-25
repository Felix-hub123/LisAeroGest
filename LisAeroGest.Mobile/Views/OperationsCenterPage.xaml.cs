using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.Views;

public partial class OperationsCenterPage : ContentPage
{
    private readonly ApiService _apiService;

    public OperationsCenterPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSummaryAsync();
    }

    private async Task LoadSummaryAsync()
    {
        var result = await _apiService.GetEmployeeOperationsSummaryAsync();

        if (!result.Success || result.Data == null)
            return;

        FlightsCount.Text = result.Data.FlightCount.ToString();
        PassengersCount.Text = result.Data.TotalPassengers.ToString();
        PendingCount.Text = result.Data.PendingCheckIns.ToString();
    }

    private Task NavigateAsync(string route) =>
        Shell.Current.GoToAsync(route);

    private void OnFlightsClicked(object sender, EventArgs e) =>
        _ = NavigateAsync("//OperationsFlightsPage");

    private void OnPassengersClicked(object sender, EventArgs e) =>
        _ = NavigateAsync(nameof(OperationsPassengersPage));

    private void OnGatesClicked(object sender, EventArgs e) =>
        _ = NavigateAsync(nameof(OperationsGatesPage));

    private void OnCommunicationsClicked(object sender, EventArgs e) =>
        _ = NavigateAsync(nameof(OperationsCommunicationsPage));

    private void OnDeskClicked(object sender, EventArgs e) =>
        _ = NavigateAsync(nameof(OperationsPassengersPage));
}