using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace LisAeroGest.Mobile.ViewModels
{
    [QueryProperty(nameof(FlightId), "flightId")]
    public partial class SelectSeatViewModel : ObservableObject
    {
        private readonly ApiService _apiService;


        // =====================================================
        // PROPRIEDADES
        // =====================================================

        [ObservableProperty]
        private int flightId;

        [ObservableProperty]
        private ObservableCollection<SeatDto> seats = new();

        [ObservableProperty]
        private SeatDto? selectedSeat;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string errorMessage = string.Empty;


        // =====================================================
        // CONSTRUTOR
        // =====================================================

        public SelectSeatViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }


        // =====================================================
        // ALTERAÇÃO DO VOO
        // =====================================================

        partial void OnFlightIdChanged(int value)
        {
            if (value > 0)
            {
                MainThread.BeginInvokeOnMainThread(
                    async () => await LoadSeatsAsync());
            }
        }


        // =====================================================
        // CARREGAR LUGARES
        // =====================================================

        [RelayCommand]
        public async Task LoadSeatsAsync()
        {
            if (IsBusy || FlightId <= 0)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var result =
                    await _apiService
                        .GetSeatsAsync(FlightId);

                if (!result.Success)
                {
                    ErrorMessage =
                        result.ErrorMessage ??
                        "Não foi possível carregar os lugares.";

                    Seats =
                        new ObservableCollection<SeatDto>();

                    return;
                }

                var loadedSeats =
                    result.Data ??
                    new List<SeatDto>();

                Seats =
                    new ObservableCollection<SeatDto>(
                        loadedSeats);

                SelectedSeat = null;
            }
            catch
            {
                ErrorMessage =
                    "Não foi possível carregar os lugares.";
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =====================================================
        // SELECIONAR LUGAR
        // =====================================================

        [RelayCommand]
        private void SelectSeat(SeatDto? seat)
        {
            if (seat == null)
                return;


            // -------------------------------------------------
            // LUGAR OCUPADO
            // -------------------------------------------------

            if (!seat.IsAvailable)
            {
                ErrorMessage =
                    $"O lugar {seat.Code} já está ocupado.";

                return;
            }


            // -------------------------------------------------
            // CLICOU NOVAMENTE NO MESMO LUGAR
            // -------------------------------------------------

            if (seat.IsSelected)
            {
                seat.IsSelected = false;

                SelectedSeat = null;

                ErrorMessage = string.Empty;

                return;
            }


            // -------------------------------------------------
            // REMOVER SELEÇÃO ANTERIOR
            // -------------------------------------------------

            foreach (var item in Seats)
            {
                item.IsSelected = false;
            }


            // -------------------------------------------------
            // NOVA SELEÇÃO
            // -------------------------------------------------

            seat.IsSelected = true;

            SelectedSeat = seat;

            ErrorMessage = string.Empty;
        }


        // =====================================================
        // CONFIRMAR LUGAR
        // =====================================================

        [RelayCommand]
        private async Task ConfirmSeatAsync()
        {
            if (SelectedSeat == null)
            {
                ErrorMessage =
                    "Selecione primeiro um lugar.";

                return;
            }

            if (!SelectedSeat.IsAvailable)
            {
                ErrorMessage =
                    "Este lugar já não está disponível.";

                await LoadSeatsAsync();

                return;
            }

            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var result =
                    await _apiService
                        .ReserveTicketAsync(
                            FlightId,
                            SelectedSeat.Id);

                if (!result.Success ||
                    result.Data == null)
                {
                    ErrorMessage =
                        result.ErrorMessage ??
                        "Não foi possível reservar o lugar.";

                    return;
                }


                // =============================================
                // RESERVA EFETUADA
                // =============================================

                await Shell.Current.GoToAsync(
                    nameof(Views.PaymentPage),
                    new Dictionary<string, object>
                    {
                        ["ticketId"] =
                            result.Data.TicketId,

                        ["flightNumber"] =
                            result.Data.FlightNumber,

                        ["seatCode"] =
                            result.Data.SeatCode,

                        ["price"] =
                            result.Data.TotalPrice.ToString(
                                "C",
                                new CultureInfo("pt-PT"))
                    });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}