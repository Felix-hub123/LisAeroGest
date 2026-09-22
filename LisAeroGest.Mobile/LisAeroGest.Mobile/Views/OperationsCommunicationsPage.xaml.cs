namespace LisAeroGest.Mobile.Views;

public partial class OperationsCommunicationsPage : ContentPage
{
    public OperationsCommunicationsPage() => InitializeComponent();
    private void OnAlertsClicked(object sender, EventArgs e) => _ = Shell.Current.GoToAsync("//NotificationsPage");
}
