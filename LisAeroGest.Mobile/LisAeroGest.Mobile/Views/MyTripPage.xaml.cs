namespace LisAeroGest.Mobile.Views
{
    public partial class MyTripPage : ContentPage
    {
        public MyTripPage()
        {
            InitializeComponent();
        }

        private async void OnVerBilhetesClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//TicketsPage");
        }
    }
}
