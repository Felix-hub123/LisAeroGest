using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class FlightBoardViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        private List<FlightDto> _allDepartures = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private string _selectedDateLabel = "Hoje";

        [ObservableProperty]
        private string _filterOrigin = string.Empty;

        [ObservableProperty]
        private string _filterDestination = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        public ObservableCollection<FlightDto> Departures { get; } = new();

        public FlightBoardViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            UpdateSelectedDateLabel();
            ApplyFilters();
        }

        [RelayCommand]
        public async Task LoadDeparturesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;

                Debug.WriteLine("[FlightBoard] ANTES da chamada API");

                var result = await _apiService.GetDeparturesAsync();

                Debug.WriteLine("[FlightBoard] DEPOIS da chamada API");

                if (result == null)
                {
                    Debug.WriteLine("[FlightBoard] RESULTADO = NULL");

                    ErrorMessage = "A API devolveu NULL.";
                    HasError = true;
                    return;
                }

                Debug.WriteLine(
                    $"[FlightBoard] Success={result.Success}");

                if (!result.Success)
                {
                    ErrorMessage =
                        result.ErrorMessage ?? "Erro ao carregar voos.";

                    HasError = true;
                    return;
                }

                _allDepartures = result.Data ?? new List<FlightDto>();

                UpdateSelectedDateLabel();
                ApplyFilters();

                Debug.WriteLine(
                    $"[FlightBoard] Voos recebidos: {_allDepartures.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[FlightBoard] EXCEÇÃO: {ex}");

                ErrorMessage =
                    $"Erro: {ex.Message}";

                HasError = true;
            }
            finally
            {
                IsBusy = false;

                Debug.WriteLine(
                    "[FlightBoard] LoadDepartures terminou.");
            }
        }

        private void ApplyFilters()
        {
            var query = _allDepartures
                .Where(f => f.DepartureTime.Date == SelectedDate.Date);

            if (!string.IsNullOrWhiteSpace(FilterOrigin))
            {
                var term = FilterOrigin.Trim();

                query = query.Where(f =>
                    f.Origin?.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase) == true);
            }

            if (!string.IsNullOrWhiteSpace(FilterDestination))
            {
                var term = FilterDestination.Trim();

                query = query.Where(f =>
                    f.Destination?.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase) == true);
            }

            var filtered = query
                .OrderBy(f => f.DepartureTime)
                .ToList();

            Departures.Clear();

            foreach (var flight in filtered)
            {
                Departures.Add(flight);
            }
        }

        partial void OnFilterOriginChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnFilterDestinationChanged(string value)
        {
            ApplyFilters();
        }

        [RelayCommand]
        private void SelectDate(DateTime date)
        {
            SelectedDate = date;
            UpdateSelectedDateLabel();
            ApplyFilters();
        }

        [RelayCommand]
        private void PreviousDay()
        {
            SelectedDate = SelectedDate.AddDays(-1);
            UpdateSelectedDateLabel();
            ApplyFilters();
        }

        [RelayCommand]
        private void NextDay()
        {
            SelectedDate = SelectedDate.AddDays(1);
            UpdateSelectedDateLabel();
            ApplyFilters();
        }

        [RelayCommand]
        private void GoToToday()
        {
            SelectedDate = DateTime.Today;
            UpdateSelectedDateLabel();
            ApplyFilters();
        }

        [RelayCommand]
        private async Task SelectFlightAsync(FlightDto? flight)
        {
            if (flight == null)
                return;

            var id = flight.Id;

            if (id <= 0)
            {
                await Shell.Current.DisplayAlert(
                    "Voo",
                    "Este voo não tem identificador válido.",
                    "OK");

                return;
            }

            try
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(Views.FlightDetailsPage)}?flightId={id}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Navegação",
                    ex.Message,
                    "OK");
            }
        }

        private void UpdateSelectedDateLabel()
        {
            var today = DateTime.Today;

            var diff =
                (SelectedDate.Date - today).Days;

            SelectedDateLabel = diff switch
            {
                0 => "Hoje",
                -1 => "Ontem",
                1 => "Amanhã",
                < -1 => $"Há {-diff} dias",
                _ => $"Em {diff} dias"
            };
        }
    }
}