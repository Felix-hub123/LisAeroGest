using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        DateLabel.Text = DateTime.Now.ToString("dd MMM yyyy");
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

    private async void OnComprarVooClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PassengerFlightsPage));
    }
}
