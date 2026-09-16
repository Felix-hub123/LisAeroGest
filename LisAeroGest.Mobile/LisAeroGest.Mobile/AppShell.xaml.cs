using LisAeroGest.Mobile.Views;


namespace LisAeroGest.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(CheckInPage), typeof(CheckInPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(FlightDetailsPage),typeof(FlightDetailsPage));
            Routing.RegisterRoute(nameof(SelectSeatPage), typeof(SelectSeatPage));



        }
    }
}
