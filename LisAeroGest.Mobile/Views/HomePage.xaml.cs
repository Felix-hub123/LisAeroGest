using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;


    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public HomePage(
        HomeViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }


    // =========================================================
    // CARREGAR HOME
    // =========================================================

    protected override async void OnAppearing()
    {
        base.OnAppearing();


        DateLabel.Text =
            DateTime.Now.ToString(
                "dd MMM yyyy");


        await _viewModel
            .LoadCommand
            .ExecuteAsync(null);
    }


    // =========================================================
    // CARTEIRA
    // =========================================================

    private async void OnVerBilhetesClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//TicketsPage");
    }


    // =========================================================
    // PESQUISAR VOOS
    // =========================================================

    private async void OnComprarVooClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(PassengerFlightsPage));
    }
}