using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Helpers;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;

namespace LisAeroGest.Mobile.ViewModels
{
    [QueryProperty(nameof(FlightId), "flightId")]
    public partial class SelectSeatViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

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

        public SelectSeatViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        partial void OnFlightIdChanged(int value)
        {
            if (value > 0)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await LoadSeatsAsync();
                });
            }
        }

        [RelayCommand]
        public async Task LoadSeatsAsync()
        {
            if (IsBusy || FlightId <= 0)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                var result = await _apiService.GetSeatsAsync(FlightId);

                if (!result.Success)
                {
                    ErrorMessage = result.ErrorMessage
                        ?? "Não foi possível carregar os lugares.";
                    Seats = new ObservableCollection<SeatDto>();
                    return;
                }

                Seats = new ObservableCollection<SeatDto>(
                    result.Data ?? new List<SeatDto>());
            }
            catch (Exception ex)
            {
                ErrorMessage = "Não foi possível carregar os lugares.";
                System.Diagnostics.Debug.WriteLine(
                    $"[SelectSeatViewModel] {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void SelectSeat(SeatDto? seat)
        {
            if (seat == null || !seat.IsAvailable)
                return;

            SelectedSeat = seat;
        }

        [RelayCommand]
        private async Task ConfirmSeatAsync()
        {
            if (SelectedSeat == null)
            {
                ErrorMessage = "Seleccione primeiro um lugar.";
                return;
            }

            await Shell.Current.DisplayAlert(
                "Reserva",
                "A compra é feita no portal web LisAeroGest.\nO lugar selecionado será confirmado após o pagamento.",
                "OK");

            await Shell.Current.GoToAsync("//TicketsPage");
        }
    }
}
