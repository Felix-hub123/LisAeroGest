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
        private string _selectedDateLabel = string.Empty;

        [ObservableProperty]
        private string _filterOrigin = string.Empty;

        [ObservableProperty]
        private string _filterDestination = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        /// <summary>
        /// Impede a seleção de dias anteriores ao dia atual.
        /// </summary>
        public DateTime MinimumDate => DateTime.Today;

        public ObservableCollection<FlightDto> Departures { get; }
            = new();

        public FlightBoardViewModel(ApiService apiService)
        {
            _apiService = apiService;

            UpdateSelectedDateLabel();
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            // Segurança adicional caso seja recebida
            // uma data anterior a hoje.
            if (value.Date < DateTime.Today)
            {
                SelectedDate = DateTime.Today;
                return;
            }

            UpdateSelectedDateLabel();
            ApplyFilters();
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
        public async Task LoadDeparturesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;

                Debug.WriteLine(
                    "[FlightBoard] A carregar voos...");

                var result =
                    await _apiService.GetDeparturesAsync();

                if (result == null)
                {
                    ErrorMessage =
                        "A API devolveu uma resposta vazia.";

                    HasError = true;
                    return;
                }

                if (!result.Success)
                {
                    ErrorMessage =
                        result.ErrorMessage ??
                        "Erro ao carregar voos.";

                    HasError = true;
                    return;
                }

                _allDepartures =
                    result.Data ?? new List<FlightDto>();

                ApplyFilters();

                Debug.WriteLine(
                    $"[FlightBoard] Voos recebidos: " +
                    $"{_allDepartures.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[FlightBoard] Erro: {ex}");

                ErrorMessage =
                    "Não foi possível carregar os voos.";

                HasError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilters()
        {
            var now = DateTime.Now;

            // Primeiro filtra pela data selecionada.
            var query = _allDepartures
                .Where(f =>
                    f.DepartureTime.Date ==
                    SelectedDate.Date);

            // Não mostrar voos que já partiram.
            query = query.Where(f =>
                f.DepartureTime > now);

            // Não mostrar voos cancelados.
            query = query.Where(f =>
                !string.Equals(
                    f.Status,
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase));

            // Filtro de origem.
            if (!string.IsNullOrWhiteSpace(FilterOrigin))
            {
                var term = FilterOrigin.Trim();

                query = query.Where(f =>
                    f.Origin?.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase)
                    == true);
            }

            // Filtro de destino.
            if (!string.IsNullOrWhiteSpace(FilterDestination))
            {
                var term = FilterDestination.Trim();

                query = query.Where(f =>
                    f.Destination?.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase)
                    == true);
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

        [RelayCommand]
        private void GoToToday()
        {
            SelectedDate = DateTime.Today;
        }

        [RelayCommand]
        private async Task SelectFlightAsync(
            FlightDto? flight)
        {
            if (flight == null)
                return;

            if (flight.Id <= 0)
            {
                await Shell.Current.DisplayAlert(
                    "Voo",
                    "Este voo não tem identificador válido.",
                    "OK");

                return;
            }

            // Proteção adicional.
            if (flight.DepartureTime <= DateTime.Now)
            {
                await Shell.Current.DisplayAlert(
                    "Voo indisponível",
                    "Este voo já partiu.",
                    "OK");

                return;
            }

            try
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(Views.FlightDetailsPage)}" +
                    $"?flightId={flight.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[FlightBoard] Navegação: {ex.Message}");

                await Shell.Current.DisplayAlert(
                    "Navegação",
                    "Não foi possível abrir este voo.",
                    "OK");
            }
        }

        private void UpdateSelectedDateLabel()
        {
            var date = SelectedDate.Date;

            if (date == DateTime.Today)
            {
                SelectedDateLabel = "Hoje";
                return;
            }

            if (date == DateTime.Today.AddDays(1))
            {
                SelectedDateLabel = "Amanhã";
                return;
            }

            SelectedDateLabel =
                date.ToString("dd MMMM yyyy");
        }
    }
}