using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;

namespace LisAeroGest.Mobile.ViewModels
{
    [QueryProperty(nameof(FlightId), "flightId")]
    public partial class SelectSeatViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty] private int flightId;
        [ObservableProperty] private ObservableCollection<SeatDto> seats = new();
        [ObservableProperty] private SeatDto? selectedSeat;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string errorMessage = string.Empty;

        public SelectSeatViewModel(ApiService apiService) => _apiService = apiService;

        partial void OnFlightIdChanged(int value)
        {
            if (value > 0)
                MainThread.BeginInvokeOnMainThread(async () => await LoadSeatsAsync());
        }

        [RelayCommand]
        public async Task LoadSeatsAsync()
        {
            if (IsBusy || FlightId <= 0) return;
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                var result = await _apiService.GetSeatsAsync(FlightId);
                if (!result.Success)
                {
                    ErrorMessage = result.ErrorMessage ?? "Não foi possível carregar os lugares.";
                    Seats = new ObservableCollection<SeatDto>();
                    return;
                }
                Seats = new ObservableCollection<SeatDto>(result.Data ?? new List<SeatDto>());
            }
            catch
            {
                ErrorMessage = "Não foi possível carregar os lugares.";
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private void SelectSeat(SeatDto? seat)
        {
            if (seat == null)
                return;

            if (!seat.IsAvailable)
            {
                ErrorMessage = "Este lugar já está ocupado.";
                return;
            }

            foreach (var item in Seats)
            {
                item.IsSelected = false;
            }

            seat.IsSelected = true;
            SelectedSeat = seat;

            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task ConfirmSeatAsync()
        {
            if (SelectedSeat == null)
            {
                ErrorMessage = "Seleccione primeiro um lugar.";
                return;
            }
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                var result = await _apiService.ReserveTicketAsync(FlightId, SelectedSeat.Id);
                if (!result.Success || result.Data == null)
                {
                    ErrorMessage = result.ErrorMessage ?? "Não foi possível reservar o lugar.";
                    return;
                }

                await Shell.Current.GoToAsync(nameof(Views.PaymentPage), new Dictionary<string, object>
                {
                    ["ticketId"] = result.Data.TicketId,
                    ["flightNumber"] = result.Data.FlightNumber,
                    ["seatCode"] = result.Data.SeatCode,
                    ["price"] = result.Data.TotalPrice.ToString("C", new System.Globalization.CultureInfo("pt-PT"))
                });
            }
            finally { IsBusy = false; }
        }
    }
}
