using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;
using System.Collections.ObjectModel;

namespace LisAeroGest.Mobile.ViewModels;

public partial class OperationsFlightsViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    private List<FlightDto> _allFlights = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusLabel = "A carregar operações…";

    public ObservableCollection<FlightDto> Flights { get; } = new();

    public OperationsFlightsViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            StatusLabel = "A carregar operações…";

            var result =
                await _apiService.GetDeparturesAsync();

            if (!result.Success)
            {
                Flights.Clear();

                StatusLabel =
                    result.ErrorMessage ??
                    "Não foi possível carregar os voos.";

                return;
            }

            _allFlights = (result.Data ?? new List<FlightDto>())
                .OrderBy(f => f.DepartureTime)
                .ToList();

            ApplyFilter("All");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Filter(string? filter)
    {
        ApplyFilter(filter ?? "All");
    }

    private void ApplyFilter(string filter)
    {
        IEnumerable<FlightDto> query = _allFlights;

        switch (filter)
        {
            case "Upcoming":

                query = query.Where(f =>
                    f.DepartureTime >= DateTime.Now &&
                    !string.Equals(
                        f.Status,
                        "Departed",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        f.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase));

                break;

            case "Boarding":

                query = query.Where(f =>
                    string.Equals(
                        f.Status,
                        "Boarding",
                        StringComparison.OrdinalIgnoreCase));

                break;

            case "Delayed":

                query = query.Where(f =>
                    string.Equals(
                        f.Status,
                        "Delayed",
                        StringComparison.OrdinalIgnoreCase));

                break;
        }

        var flights = query.ToList();

        Flights.Clear();

        foreach (var flight in flights)
        {
            Flights.Add(flight);
        }

        StatusLabel = flights.Count switch
        {
            0 => "Nenhum voo encontrado.",
            1 => "1 voo em operação.",
            _ => $"{flights.Count} voos em operação."
        };
    }

    [RelayCommand]
    private async Task ViewOperationAsync(FlightDto? flight)
    {
        if (flight == null)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(OperationsFlightDetailsPage)}?flightId={flight.Id}");
    }
}