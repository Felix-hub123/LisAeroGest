using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class PaymentViewModel : ObservableObject, IQueryAttributable
    {
        private readonly ApiService _apiService;

        [ObservableProperty] private int ticketId;
        [ObservableProperty] private int flightId;
        [ObservableProperty] private string seatCode = string.Empty;
        [ObservableProperty] private string flightNumber = string.Empty;
        [ObservableProperty] private string price = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool paymentStarted;
        private string _orderId = string.Empty;
        private string _approvalUrl = string.Empty;

        public PaymentViewModel(ApiService apiService) => _apiService = apiService;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Debug.WriteLine("PAYMENT QUERY: " + string.Join(", ", query.Select(kv => kv.Key + "=" + kv.Value)));
            if (query.TryGetValue("ticketId", out var tid)) TicketId = Convert.ToInt32(tid.ToString());
            if (query.TryGetValue("flightId", out var fid)) FlightId = Convert.ToInt32(fid.ToString());
            if (query.TryGetValue("seatCode", out var code)) SeatCode = Uri.UnescapeDataString(code?.ToString() ?? "");
            if (query.TryGetValue("flightNumber", out var fn)) FlightNumber = Uri.UnescapeDataString(fn?.ToString() ?? "");
            if (query.TryGetValue("price", out var p)) Price = Uri.UnescapeDataString(p?.ToString() ?? "");
        }

        [RelayCommand]
        private async Task StartPaymentAsync()
        {
            if (IsBusy || TicketId <= 0) return;
            try
            {
                IsBusy = true;
                StatusMessage = "A preparar o pagamento seguro…";
                var result = await _apiService.CreatePayPalOrderAsync(TicketId);
                if (!result.Success || result.Data == null)
                {
                    StatusMessage = result.ErrorMessage ?? "Não foi possível iniciar o pagamento.";
                    return;
                }

                _orderId = result.Data.OrderId;
                _approvalUrl = result.Data.ApprovalUrl;
                PaymentStarted = true;
                StatusMessage = "O PayPal foi aberto. Conclui o pagamento e regressa à aplicação para confirmar.";
                await Browser.Default.OpenAsync(_approvalUrl);
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task ConfirmPaymentAsync()
        {
            if (IsBusy || TicketId <= 0 || string.IsNullOrWhiteSpace(_orderId))
            {
                StatusMessage = "Inicia primeiro o pagamento no PayPal.";
                return;
            }
            try
            {
                IsBusy = true;
                StatusMessage = "A confirmar pagamento…";
                var result = await _apiService.CapturePayPalOrderAsync(TicketId, _orderId);
                if (!result.Success || result.Data == null)
                {
                    StatusMessage = result.ErrorMessage ?? "O pagamento ainda não foi confirmado. Se acabaste de pagar, aguarda alguns segundos e tenta novamente.";
                    return;
                }

                await Shell.Current.DisplayAlert("Pagamento concluído", "Bilhete comprado com sucesso. A tua viagem já está disponível na carteira.", "OK");
                await Shell.Current.GoToAsync("//TicketsPage");
            }
            finally { IsBusy = false; }
        }
    }
}
