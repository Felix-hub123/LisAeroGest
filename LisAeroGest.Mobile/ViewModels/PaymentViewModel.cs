using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class PaymentViewModel :
        ObservableObject,
        IQueryAttributable
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private int ticketId;

        [ObservableProperty]
        private int flightId;

        [ObservableProperty]
        private string seatCode = string.Empty;

        [ObservableProperty]
        private string flightNumber = string.Empty;

        [ObservableProperty]
        private string price = string.Empty;

        [ObservableProperty]
        private string phoneNumber = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool paymentRequestSent;

        [ObservableProperty]
        private bool paymentCompleted;

        public PaymentViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            Debug.WriteLine(
                "PAYMENT QUERY: " +
                string.Join(
                    ", ",
                    query.Select(kv =>
                        kv.Key + "=" + kv.Value)));

            if (query.TryGetValue(
                "ticketId",
                out var ticketIdValue))
            {
                TicketId =
                    Convert.ToInt32(
                        ticketIdValue.ToString());
            }

            if (query.TryGetValue(
                "flightId",
                out var flightIdValue))
            {
                FlightId =
                    Convert.ToInt32(
                        flightIdValue.ToString());
            }

            if (query.TryGetValue(
                "seatCode",
                out var seatCodeValue))
            {
                SeatCode =
                    Uri.UnescapeDataString(
                        seatCodeValue?.ToString() ?? "");
            }

            if (query.TryGetValue(
                "flightNumber",
                out var flightNumberValue))
            {
                FlightNumber =
                    Uri.UnescapeDataString(
                        flightNumberValue?.ToString() ?? "");
            }

            if (query.TryGetValue(
                "price",
                out var priceValue))
            {
                Price =
                    Uri.UnescapeDataString(
                        priceValue?.ToString() ?? "");
            }
        }

        [RelayCommand]
        private async Task StartPaymentAsync()
        {
            if (IsBusy || TicketId <= 0)
                return;

            var normalizedPhone =
                NormalizePhoneNumber(PhoneNumber);

            if (!IsValidPhoneNumber(normalizedPhone))
            {
                StatusMessage =
                    "Introduz um número de telemóvel português válido.";

                return;
            }

            try
            {
                IsBusy = true;

                StatusMessage =
                    "A enviar pedido MB WAY…";

                // Simulates receiving the request
                // on the MB WAY application.
                await Task.Delay(1000);

                PaymentRequestSent = true;

                StatusMessage =
                    $"Pedido enviado para {MaskPhoneNumber(normalizedPhone)}. " +
                    "Autoriza o pagamento para continuar.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ConfirmPaymentAsync()
        {
            if (IsBusy || TicketId <= 0)
                return;

            if (!PaymentRequestSent)
            {
                StatusMessage =
                    "Envia primeiro o pedido MB WAY.";

                return;
            }

            try
            {
                IsBusy = true;

                StatusMessage =
                    "A processar pagamento…";

                var result =
                    await _apiService.PayWithMbWayAsync(
                        TicketId,
                        PhoneNumber);

                if (!result.Success ||
                    result.Data == null)
                {
                    StatusMessage =
                        result.ErrorMessage ??
                        "Não foi possível confirmar o pagamento.";

                    return;
                }

                PaymentCompleted = true;

                StatusMessage =
                    "Pagamento concluído.";

                await Shell.Current.DisplayAlert(
                    "Pagamento concluído",
                    $"Pagamento de {result.Data.Amount:F2} € " +
                    "simulado com sucesso.\n\n" +
                    $"Referência: {result.Data.TransactionId}",
                    "Ver bilhete");

                await Shell.Current.GoToAsync(
                    "//TicketsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[Payment] Error: {ex}");

                StatusMessage =
                    "Ocorreu um erro durante o pagamento.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string NormalizePhoneNumber(
            string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            var normalized =
                phoneNumber
                    .Replace(" ", "")
                    .Replace("-", "");

            if (normalized.StartsWith("+351"))
                normalized = normalized[4..];

            return normalized;
        }

        private static bool IsValidPhoneNumber(
            string phoneNumber)
        {
            return phoneNumber.Length == 9 &&
                   phoneNumber.All(char.IsDigit) &&
                   phoneNumber.StartsWith("9");
        }

        private static string MaskPhoneNumber(
            string phoneNumber)
        {
            if (phoneNumber.Length != 9)
                return phoneNumber;

            return
                $"{phoneNumber[..3]} *** {phoneNumber[^3..]}";
        }
    }
}