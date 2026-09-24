using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;

namespace LisAeroGest.Mobile.ViewModels;

public partial class OperationsGatesViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string _statusLabel = "A carregar…";

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<FlightDto> GatesFlights { get; } = new();

    public OperationsGatesViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusLabel = "A carregar…";

            var result = await _apiService.GetDeparturesAsync();

            if (!result.Success)
            {
                GatesFlights.Clear();
                StatusLabel = result.ErrorMessage
                    ?? "Não foi possível carregar as portas.";
                return;
            }

            // Só voos com porta já atribuída pelo servidor
            var flightsWithGate = (result.Data ?? new List<FlightDto>())
                .Where(f => !string.IsNullOrWhiteSpace(f.Gate))
                .OrderBy(f => f.Gate)
                .ThenBy(f => f.DepartureTime)
                .ToList();

            GatesFlights.Clear();
            foreach (var flight in flightsWithGate)
                GatesFlights.Add(flight);

            StatusLabel = flightsWithGate.Count == 0
                ? "Nenhum voo com porta atribuída de momento."
                : $"{flightsWithGate.Count} voo(s) com porta atribuída.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
