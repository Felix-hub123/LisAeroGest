using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class MyTripPage : ContentPage
{
    private readonly MyTripViewModel _viewModel;

    public MyTripPage(MyTripViewModel viewModel)
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

    private async void OnVerBilhetesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//TicketsPage");
    }

    private async void OnCheckInClicked(object sender, EventArgs e)
    {
        if (_viewModel.CurrentTrip == null) return;

        await Shell.Current.GoToAsync(
            nameof(CheckInPage),
            new Dictionary<string, object>
            {
                ["ticketId"] = _viewModel.CurrentTrip.Id
            });
    }
}
