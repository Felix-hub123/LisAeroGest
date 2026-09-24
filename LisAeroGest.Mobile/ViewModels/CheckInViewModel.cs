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
        private ImageSource? _qrCodeImage;

        /// <summary>
        /// Indica se o passageiro ainda não realizou o check-in.
        /// Utilizado pelo XAML para mostrar o botão de check-in.
        /// </summary>
        public bool NotCheckedIn => !CheckInDone;

        public CheckInViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// É chamado automaticamente quando o TicketId é recebido
        /// através da navegação do Shell.
        /// </summary>
        partial void OnTicketIdChanged(int value)
        {
            if (value <= 0)
                return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LoadBoardingPassAsync();
            });
        }

        /// <summary>
        /// Atualiza a propriedade NotCheckedIn sempre que
        /// CheckInDone mudar.
        /// </summary>
        partial void OnCheckInDoneChanged(bool value)
        {
            OnPropertyChanged(nameof(NotCheckedIn));
        }

        /// <summary>
        /// Procura no servidor um cartão de embarque
        /// já existente para este bilhete.
        /// </summary>
        private async Task LoadBoardingPassAsync()
        {
            if (TicketId <= 0 || IsBusy)
                return;

            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var result =
                    await _apiService.GetBoardingPassAsync(TicketId);

                if (result.Success && result.Data != null)
                {
                    FlightNumber = result.Data.FlightNumber;
                    Gate = result.Data.Gate;
                    SequenceNumber = result.Data.SequenceNumber;

                    GenerateQrCode(result.Data.QRData);

                    CheckInDone = true;

                    Debug.WriteLine(
                        $"[CheckInViewModel] Boarding pass encontrado para Ticket {TicketId}.");
                }
                else
                {
                   
                    CheckInDone = false;
                    QrCodeImage = null;

                    Debug.WriteLine(
                        $"[CheckInViewModel] Boarding pass não encontrado para Ticket {TicketId}. " +
                        $"Resposta: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[CheckInViewModel] Erro ao carregar BoardingPass: {ex.Message}");

                CheckInDone = false;
                QrCodeImage = null;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Realiza o check-in do passageiro.
        /// </summary>
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

                var result =
                    await _apiService.DoCheckInAsync(TicketId);

                if (result.Success)
                {
                    FlightNumber = result.FlightNumber;
                    Gate = result.Gate;
                    SequenceNumber = result.SequenceNumber;

                    GenerateQrCode(result.QRData);

                    CheckInDone = true;
                }
                else
                {
                    ErrorMessage =
                        result.ErrorMessage ??
                        "Não foi possível fazer o check-in.";

                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[CheckInViewModel] Erro no check-in: {ex.Message}");

                ErrorMessage =
                    "Ocorreu um erro de ligação ao servidor.";

                HasError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Gera a imagem QR Code a partir dos dados
        /// recebidos do servidor.
        /// </summary>
        private void GenerateQrCode(string? qrData)
        {
            if (string.IsNullOrWhiteSpace(qrData))
            {
                QrCodeImage = null;
                return;
            }

            try
            {
                var qrBytes =
                    Helpers.QrCodeGenerator.GeneratePng(
                        qrData,
                        400);

                if (qrBytes.Length == 0)
                {
                    QrCodeImage = null;
                    return;
                }

                QrCodeImage =
                    ImageSource.FromStream(
                        () => new MemoryStream(qrBytes));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[CheckInViewModel] Erro ao gerar QR Code: {ex.Message}");

                QrCodeImage = null;
            }
        }
    }
}