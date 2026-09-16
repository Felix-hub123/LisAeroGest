using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;

namespace LisAeroGest.Mobile.ViewModels
{
    [QueryProperty(nameof(FlightId), "flightId")]
    public partial class FlightDetailsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private int flightId;

        [ObservableProperty]
        private FlightDetailDto? flight;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public FlightDetailsViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        partial void OnFlightIdChanged(int value)
        {
            if (value > 0)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await LoadAsync();
                });
            }
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (FlightId <= 0 || IsBusy)
                return;

            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                Flight = await _apiService.GetDetailsAsync(FlightId);

                if (Flight == null)
                {
                    ErrorMessage =
                        "Não foi possível encontrar os detalhes do voo.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    "Ocorreu um erro ao carregar os detalhes do voo.";

                System.Diagnostics.Debug.WriteLine(
                    $"[FlightDetailsViewModel] {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SelectSeatAsync()
        {
            if (FlightId <= 0)
                return;

            await Shell.Current.GoToAsync(
                nameof(SelectSeatPage),
                new Dictionary<string, object>
                {
                    ["flightId"] = FlightId
                });
        }
    }
}
