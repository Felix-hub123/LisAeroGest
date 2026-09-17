using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class PaymentViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty] private int flightId;
        [ObservableProperty] private int seatId;
        [ObservableProperty] private string seatCode = string.Empty;
        [ObservableProperty] private string flightNumber = string.Empty;
        [ObservableProperty] private string price = string.Empty;
        [ObservableProperty] private bool isBusy;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Debug.WriteLine("QUERY: " + string.Join(", ", query.Select(kv => kv.Key + "=" + kv.Value)));
            if (query.TryGetValue("flightId", out var fid))
                FlightId = Convert.ToInt32(fid.ToString());

            if (query.TryGetValue("seatId", out var sid))
                SeatId = Convert.ToInt32(sid.ToString());

            if (query.TryGetValue("seatCode", out var code))
                SeatCode = Uri.UnescapeDataString(code.ToString() ?? "");

            if (query.TryGetValue("flightNumber", out var fn))
                FlightNumber = Uri.UnescapeDataString(fn.ToString() ?? "");

            if (query.TryGetValue("price", out var p))
                Price = Uri.UnescapeDataString(p.ToString() ?? "");

            OnPropertyChanged(nameof(Summary));
        }

        public string Summary =>
            $"{FlightNumber}  ·  Lugar {SeatCode}  ·  {Price}";

        [RelayCommand]
        private async Task PayAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                // Próximo passo: chamar a API de reserva.
                // Por agora confirma o fluxo na defesa.
                await Shell.Current.DisplayAlert(
                    "Pagamento",
                    $"Reserva {FlightNumber}, lugar {SeatCode}, {Price}.\n(API de pagamento no passo seguinte)",
                    "OK");

                await Shell.Current.GoToAsync("//TicketsPage");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}