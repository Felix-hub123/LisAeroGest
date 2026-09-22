namespace LisAeroGest.Mobile.Views;

public partial class OperationsCenterPage : ContentPage
{
    public OperationsCenterPage() => InitializeComponent();

    private Task NavigateAsync(string route) => Shell.Current.GoToAsync(route);
    private void OnFlightsClicked(object sender, EventArgs e) => _ = NavigateAsync("//FlightBoardPage");
    private void OnPassengersClicked(object sender, EventArgs e) => _ = NavigateAsync(nameof(OperationsPassengersPage));
    private void OnGatesClicked(object sender, EventArgs e) => _ = NavigateAsync(nameof(OperationsGatesPage));
    private void OnCommunicationsClicked(object sender, EventArgs e) => _ = NavigateAsync(nameof(OperationsCommunicationsPage));
    private void OnDeskClicked(object sender, EventArgs e) => _ = NavigateAsync(nameof(OperationsPassengersPage));
}
