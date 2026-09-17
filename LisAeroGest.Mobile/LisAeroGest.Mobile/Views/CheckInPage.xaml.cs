using LisAeroGest.Mobile.Helpers;
using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class CheckInPage : ContentPage
    {
        public CheckInPage()
        {
            InitializeComponent();
            var services = Application.Current?.Handler?.MauiContext?.Services;
            if (services != null)
                BindingContext = services.GetService<CheckInViewModel>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is CheckInViewModel vm && vm.TicketId <= 0)
                vm.TicketId = PendingBooking.TicketId;
        }
    }
}