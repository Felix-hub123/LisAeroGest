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


        // =========================================================
        // DADOS DA RESERVA
        // =========================================================

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


        // =========================================================
        // PAGAMENTO
        // =========================================================

        [ObservableProperty]
        private string phoneNumber = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool paymentRequestSent;

        [ObservableProperty]
        private bool paymentCompleted;


        // =========================================================
        // FEEDBACK
        // =========================================================

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string successMessage = string.Empty;

        [ObservableProperty]
        private bool hasSuccess;


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public PaymentViewModel(
            ApiService apiService)
        {
            _apiService = apiService;
        }


        // =========================================================
        // RECEBER PARÂMETROS
        // =========================================================

        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            Debug.WriteLine(
                "PAYMENT QUERY: " +
                string.Join(
                    ", ",
                    query.Select(kv =>
                        kv.Key + "=" + kv.Value)));

            ClearFeedback();

            PaymentRequestSent = false;
            PaymentCompleted = false;


            // =====================================================
            // TICKET
            // =====================================================

            if (query.TryGetValue(
                "ticketId",
                out var ticketIdValue)
                &&
                int.TryParse(
                    ticketIdValue?.ToString(),
                    out var parsedTicketId))
            {
                TicketId = parsedTicketId;
            }


            // =====================================================
            // VOO
            // =====================================================

            if (query.TryGetValue(
                "flightId",
                out var flightIdValue)
                &&
                int.TryParse(
                    flightIdValue?.ToString(),
                    out var parsedFlightId))
            {
                FlightId = parsedFlightId;
            }


            // =====================================================
            // LUGAR
            // =====================================================

            if (query.TryGetValue(
                "seatCode",
                out var seatCodeValue))
            {
                SeatCode =
                    Uri.UnescapeDataString(
                        seatCodeValue?.ToString() ??
                        string.Empty);
            }


            // =====================================================
            // NÚMERO DO VOO
            // =====================================================

            if (query.TryGetValue(
                "flightNumber",
                out var flightNumberValue))
            {
                FlightNumber =
                    Uri.UnescapeDataString(
                        flightNumberValue?.ToString() ??
                        string.Empty);
            }


            // =====================================================
            // PREÇO
            // =====================================================

            if (query.TryGetValue(
                "price",
                out var priceValue))
            {
                Price =
                    Uri.UnescapeDataString(
                        priceValue?.ToString() ??
                        string.Empty);
            }
        }


        // =========================================================
        // LIMPAR FEEDBACK
        // =========================================================

        private void ClearFeedback()
        {
            HasError = false;
            ErrorMessage = string.Empty;

            HasSuccess = false;
            SuccessMessage = string.Empty;

            StatusMessage = string.Empty;
        }


        // =========================================================
        // MOSTRAR ERRO
        // =========================================================

        private void ShowError(
            string message)
        {
            StatusMessage = string.Empty;

            HasSuccess = false;
            SuccessMessage = string.Empty;

            ErrorMessage = message;
            HasError = true;
        }


        // =========================================================
        // MOSTRAR SUCESSO
        // =========================================================

        private void ShowSuccess(
            string message)
        {
            StatusMessage = string.Empty;

            HasError = false;
            ErrorMessage = string.Empty;

            SuccessMessage = message;
            HasSuccess = true;
        }


        // =========================================================
        // ENVIAR PEDIDO MB WAY
        // =========================================================

        [RelayCommand]
        private async Task StartPaymentAsync()
        {
            if (IsBusy || TicketId <= 0)
                return;

            ClearFeedback();


            // =====================================================
            // NORMALIZAR TELEFONE
            // =====================================================

            var normalizedPhone =
                NormalizePhoneNumber(
                    PhoneNumber);


            // =====================================================
            // VALIDAR TELEFONE
            // =====================================================

            if (!IsValidPhoneNumber(
                normalizedPhone))
            {
                ShowError(
                    "Introduz um número de telemóvel português válido.");

                return;
            }


            // Guardamos o número normalizado.
            PhoneNumber = normalizedPhone;


            // =====================================================
            // SIMULAR PEDIDO MB WAY
            // =====================================================

            try
            {
                IsBusy = true;

                StatusMessage =
                    "A enviar pedido MB WAY…";

                await Task.Delay(1000);

                PaymentRequestSent = true;

                StatusMessage = string.Empty;

                ShowSuccess(
                    $"Pedido enviado para " +
                    $"{MaskPhoneNumber(normalizedPhone)}. " +
                    $"Autoriza o pagamento para concluir a compra.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[Payment] Start payment error: {ex}");

                ShowError(
                    "Não foi possível enviar o pedido MB WAY.");
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // CONFIRMAR PAGAMENTO
        // =========================================================

        [RelayCommand]
        private async Task ConfirmPaymentAsync()
        {
            if (IsBusy || TicketId <= 0)
                return;


            // =====================================================
            // VALIDAR PEDIDO
            // =====================================================

            if (!PaymentRequestSent)
            {
                ShowError(
                    "Envia primeiro o pedido MB WAY.");

                return;
            }


            // =====================================================
            // CONFIRMAÇÃO
            // =====================================================

            var confirmed =
                await Shell.Current.DisplayAlert(
                    "Autorizar pagamento",
                    $"Confirmar o pagamento de {Price} " +
                    $"para o voo {FlightNumber}?",
                    "Autorizar",
                    "Cancelar");

            if (!confirmed)
                return;


            // =====================================================
            // PROCESSAR PAGAMENTO
            // =====================================================

            try
            {
                IsBusy = true;

                HasError = false;
                ErrorMessage = string.Empty;

                HasSuccess = false;
                SuccessMessage = string.Empty;

                StatusMessage =
                    "A processar pagamento…";

                var result =
                    await _apiService
                        .PayWithMbWayAsync(
                            TicketId,
                            PhoneNumber);


                // =================================================
                // ERRO
                // =================================================

                if (!result.Success ||
                    result.Data == null)
                {
                    ShowError(
                        result.ErrorMessage ??
                        "Não foi possível confirmar o pagamento.");

                    return;
                }


                // =================================================
                // PAGAMENTO CONCLUÍDO
                // =================================================

                PaymentCompleted = true;

                PaymentRequestSent = false;

                ShowSuccess(
                    $"Pagamento de " +
                    $"{result.Data.Amount:F2} € " +
                    $"concluído com sucesso. " +
                    $"Referência: " +
                    $"{result.Data.TransactionId}");


                // =================================================
                // PEQUENA PAUSA PARA MOSTRAR O RESULTADO
                // =================================================

                await Task.Delay(900);


                // =================================================
                // IR PARA OS BILHETES
                // =================================================

                await Shell.Current.GoToAsync(
                    "//TicketsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[Payment] Error: {ex}");

                ShowError(
                    "Ocorreu um erro durante o pagamento.");
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // NORMALIZAR NÚMERO
        // =========================================================

        private static string NormalizePhoneNumber(
            string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(
                phoneNumber))
            {
                return string.Empty;
            }

            var normalized =
                phoneNumber
                    .Replace(" ", "")
                    .Replace("-", "");

            if (normalized.StartsWith("+351"))
            {
                normalized =
                    normalized[4..];
            }

            return normalized;
        }


        // =========================================================
        // VALIDAR NÚMERO
        // =========================================================

        private static bool IsValidPhoneNumber(
            string phoneNumber)
        {
            return
                phoneNumber.Length == 9 &&
                phoneNumber.All(char.IsDigit) &&
                phoneNumber.StartsWith("9");
        }


        // =========================================================
        // MASCARAR NÚMERO
        // =========================================================

        private static string MaskPhoneNumber(
            string phoneNumber)
        {
            if (phoneNumber.Length != 9)
                return phoneNumber;

            return
                $"{phoneNumber[..3]} *** " +
                $"{phoneNumber[^3..]}";
        }
    }
}