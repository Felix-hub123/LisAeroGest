using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class TicketsPage : ContentPage
{
    private readonly TicketsViewModel _viewModel;

    public TicketsPage(TicketsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadTicketsCommand.ExecuteAsync(null);
    }

    private async void OnCheckInClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is TicketDto ticket)
        {
            await Shell.Current.GoToAsync($"{nameof(CheckInPage)}?ticketId={ticket.Id}");
        }
    }
}