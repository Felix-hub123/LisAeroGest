using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class FlightDetailsViewModel :
        ObservableObject,
        IQueryAttributable
    {
        private readonly ApiService _apiService;
        private readonly FavoritesService _favoritesService;

        [ObservableProperty]
        private int flightId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasFlight))]
        private FlightDetailDto? flight;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isFavorite;

        public bool HasFlight => Flight != null;

        public FlightDetailsViewModel(
            ApiService apiService,
            FavoritesService favoritesService)
        {
            _apiService = apiService;
            _favoritesService = favoritesService;
        }

        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            if (!query.TryGetValue(
                    "flightId",
                    out var raw) ||
                raw == null)
            {
                return;
            }

            if (raw is int value)
            {
                FlightId = value;
            }
            else if (int.TryParse(
                         raw.ToString(),
                         out var parsed))
            {
                FlightId = parsed;
            }
        }

        partial void OnFlightIdChanged(int value)
        {
            if (value <= 0)
                return;

            MainThread.BeginInvokeOnMainThread(
                async () => await LoadAsync());
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

                var result =
                    await _apiService.GetDetailsAsync(FlightId);

                if (!result.Success ||
                    result.Data == null)
                {
                    Flight = null;

                    ErrorMessage =
                        result.ErrorMessage ??
                        "Não foi possível encontrar os detalhes do voo.";

                    return;
                }

                Flight = result.Data;

                IsFavorite =
                    _favoritesService.IsFavorite(Flight.Id);
            }
            catch (Exception ex)
            {
                Flight = null;

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
        private void ToggleFavorite()
        {
            if (Flight == null)
                return;

            IsFavorite =
                _favoritesService.Toggle(Flight.Id);
        }

        [RelayCommand]
        private async Task SelectSeatAsync()
        {
            if (FlightId <= 0)
                return;

            await Shell.Current.GoToAsync(
                $"{nameof(SelectSeatPage)}?flightId={FlightId}");
        }
    }
}