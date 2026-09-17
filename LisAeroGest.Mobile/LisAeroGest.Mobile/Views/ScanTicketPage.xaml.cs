namespace LisAeroGest.Mobile.Views
{
    public partial class ScanTicketPage : ContentPage
    {
        private bool _handled;

        public ScanTicketPage()
        {
            InitializeComponent();
        }

        private async void OnBarcodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            var value = e.Results?.FirstOrDefault()?.Value;
            if (_handled || string.IsNullOrWhiteSpace(value))
                return;

            _handled = true;
            Camera.IsDetecting = false;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                ResultLabel.Text = value;
                await DisplayAlert("QR lido", value, "OK");
                _handled = false;
                Camera.IsDetecting = true;
            });
        }
    }
}