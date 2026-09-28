using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class TicketsPage : ContentPage
    {
        private readonly TicketsViewModel _viewModel;


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public TicketsPage(
            TicketsViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;
        }


        // =========================================================
        // CARREGAR BILHETES
        // =========================================================

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel
                .LoadTicketsCommand
                .ExecuteAsync(null);
        }


        // =========================================================
        // CHECK-IN
        // =========================================================

        private async void OnCheckInClicked(
            object sender,
            EventArgs e)
        {
            if (sender is not Button button ||
                button.BindingContext is not TicketDto ticket)
            {
                return;
            }


            await Shell.Current.GoToAsync(
                nameof(CheckInPage),
                new Dictionary<string, object>
                {
                    ["ticketId"] = ticket.Id
                });
        }
    }
}