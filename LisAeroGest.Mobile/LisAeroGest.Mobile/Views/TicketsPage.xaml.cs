using LisAeroGest.Mobile.Helpers;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class TicketsPage : ContentPage
    {
        public TicketsPage(TicketsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is TicketsViewModel vm)
                await vm.LoadTicketsCommand.ExecuteAsync(null);
        }

        private async void OnCheckInClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is TicketDto ticket)
            {
                PendingBooking.TicketId = ticket.Id;
                await Shell.Current.GoToAsync(nameof(CheckInPage));
            }
        }

       
    }
}