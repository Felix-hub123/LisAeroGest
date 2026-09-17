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

                var result = await _apiService.GetDetailsAsync(FlightId);

                if (!result.Success)
                {
                    Flight = null;
                    ErrorMessage = result.ErrorMessage
                        ?? "Não foi possível encontrar os detalhes do voo.";
                    return;
                }

                Flight = result.Data;

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

        /// <summary>
        /// Navega para a página de seleção de lugar deste voo.
        /// </summary>
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