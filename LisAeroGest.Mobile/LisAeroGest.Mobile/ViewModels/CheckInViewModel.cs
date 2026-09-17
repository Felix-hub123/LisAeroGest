using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels
{

    [QueryProperty(nameof(TicketId), "ticketId")]
    public partial class CheckInViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private int _ticketId;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _checkInDone;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private string _flightNumber = string.Empty;

        [ObservableProperty]
        private string _gate = string.Empty;

        [ObservableProperty]
        private int _sequenceNumber;

        [ObservableProperty]
        private string _qrData = string.Empty;

        [ObservableProperty]
        private string? qrImageUrl;

        /// <summary>
        /// Propriedade calculada — o inverso de CheckInDone, usada no XAML
        /// para mostrar o botão de confirmação antes do check-in ser feito.
        /// </summary>
        public bool NotCheckedIn => !CheckInDone;

        /// <summary>
        /// Chamado automaticamente pelo CommunityToolkit sempre que CheckInDone muda,
        /// para avisar a UI de que NotCheckedIn também mudou.
        /// </summary>
        partial void OnCheckInDoneChanged(bool value)
        {
            OnPropertyChanged(nameof(NotCheckedIn));
        }

        public CheckInViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task ConfirmCheckInAsync()
        {
            if (IsBusy)
                return;

            if (TicketId <= 0)
            {
                ErrorMessage = "Bilhete inválido.";
                HasError = true;
                return;
            }

            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var result = await _apiService.DoCheckInAsync(TicketId);

                if (result.Success)
                {
                    FlightNumber = result.FlightNumber;
                    Gate = result.Gate;
                    SequenceNumber = result.SequenceNumber;
                    QrData = result.QRData;

                    var contentToEncode = !string.IsNullOrWhiteSpace(result.QRData)
                        ? result.QRData
                        : $"BOARDING|{TicketId}|{FlightNumber}";

                    QrImageUrl =
                        $"https://quickchart.io/qr?text={Uri.EscapeDataString(contentToEncode)}";

                    CheckInDone = true;
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Não foi possível fazer o check-in.";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckInViewModel] {ex.Message}");

                ErrorMessage = "Ocorreu um erro de ligação ao servidor.";
                HasError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

