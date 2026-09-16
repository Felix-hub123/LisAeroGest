using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class FlightBoardViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private string _selectedDateLabel = "Hoje";

        public ObservableCollection<FlightDto> Departures { get; } = new();

        public FlightBoardViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        public async Task LoadDeparturesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var flights = await _apiService.GetDeparturesAsync();

                UpdateSelectedDateLabel();

                var flightsOfDay = flights
                    .Where(f => f.DepartureTime.Date == SelectedDate.Date)
                    .OrderBy(f => f.DepartureTime)
                    .ToList();

                Departures.Clear();
                foreach (var flight in flightsOfDay)
                {
                    Departures.Add(flight);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FlightBoard] Erro: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SelectDateAsync(DateTime date)
        {
            SelectedDate = date;
            await LoadDeparturesCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task PreviousDayAsync()
        {
            SelectedDate = SelectedDate.AddDays(-1);
            await LoadDeparturesCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task NextDayAsync()
        {
            SelectedDate = SelectedDate.AddDays(1);
            await LoadDeparturesCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        private async Task GoToTodayAsync()
        {
            SelectedDate = DateTime.Today;
            await LoadDeparturesCommand.ExecuteAsync(null);
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
                    nameof(Views.FlightDetailsPage),
                    new Dictionary<string, object>
                    {
                        ["FlightId"] = id
                    });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Navegação", ex.Message, "OK");
            }
        }



        private void UpdateSelectedDateLabel()
        {
            var today = DateTime.Today;
            var diff = (SelectedDate.Date - today).Days;

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