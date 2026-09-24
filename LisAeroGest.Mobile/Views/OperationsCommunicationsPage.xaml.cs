using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class OperationsCommunicationsPage : ContentPage
{
    private readonly OperationsCommunicationsViewModel _viewModel;

    public OperationsCommunicationsPage(OperationsCommunicationsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private async void OnAlertsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//NotificationsPage");
    }
}
