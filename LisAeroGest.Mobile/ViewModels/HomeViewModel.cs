using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private List<FlightDto> _allDepartures = new();

    // ── Status ────────────────────────────────────────────────────────────────
    [ObservableProperty]
    private string _statusLabel = "A carregar dados da tua conta…";

    // ── Voo 1 ─────────────────────────────────────────────────────────────────
    [ObservableProperty]
    private string _flightOneNumber = "—";

    [ObservableProperty]
    private string _flightOneDestination = "Sem dados";

    [ObservableProperty]
    private string _flightOneTime = "—";

    [ObservableProperty]
    private string _flightOneGate = "—";

    // ── Voo 2 ─────────────────────────────────────────────────────────────────
    [ObservableProperty]
    private string _flightTwoNumber = "—";

    [ObservableProperty]
    private string _flightTwoDestination = "Sem dados";

    [ObservableProperty]
    private string _flightTwoTime = "—";

    [ObservableProperty]
    private string _flightTwoGate = "—";

    // ── Boarding Summary ───────────────────────────────────────────────────────
    [ObservableProperty]
    private string _boardingFlight = "—";

    [ObservableProperty]
    private string _boardingSeat = "—";

    [ObservableProperty]
    private bool _boardingSummaryIsVisible;

    [ObservableProperty]
    private bool _noBoardingLabelIsVisible = true;

    public HomeViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        StatusLabel = "A carregar dados da tua conta…";

        var departuresTask = _apiService.GetDeparturesAsync();
        var ticketsTask = _apiService.GetMyTicketsAsync();
        await Task.WhenAll(departuresTask, ticketsTask);

        var departures = departuresTask.Result;
        var tickets = ticketsTask.Result;

        Debug.WriteLine(
            $"[HomeViewModel] Departures -> Success={departures?.Success}, "
            + $"Count={departures?.Data?.Count ?? 0}, "
            + $"Error={departures?.ErrorMessage}");

        Debug.WriteLine(
            $"[HomeViewModel] Tickets -> Success={tickets?.Success}, "
            + $"Count={tickets?.Data?.Count ?? 0}, "
            + $"Error={tickets?.ErrorMessage}");

        if (departures?.Data != null)
        {
            foreach (var f in departures.Data.Take(3))
            {
                Debug.WriteLine(
                    $"[HomeViewModel] Voo: {f.FlightNumber} | "
                    + $"{f.Origin}->{f.Destination} | "
                    + $"Departure={f.DepartureTime:O} | "
                    + $"Gate={f.Gate} | Status={f.Status}");
            }
        }

        if (!departures!.Success && !tickets!.Success)
        {
            StatusLabel = departures.ErrorMessage
                ?? tickets.ErrorMessage
                ?? "Não foi possível carregar os dados.";
            ClearFlight1();
            ClearFlight2();
            BoardingSummaryIsVisible = false;
            NoBoardingLabelIsVisible = true;
            return;
        }

        // ── Preenche os 2 voos mais próximos ───────────────────────────────
        var flights = (departures.Data ?? new List<FlightDto>())
            .OrderBy(f => f.DepartureTime)
            .Take(2)
            .ToList();

        var f1 = flights.ElementAtOrDefault(0);
        var f2 = flights.ElementAtOrDefault(1);

        SetFlight1(f1);
        SetFlight2(f2);

        // ── Boarding pass summary ────────────────────────────────────────────
        var boarding = (tickets.Data ?? new List<TicketDto>())
            .FirstOrDefault(t => t.Status == "CheckedIn" || t.BoardingPassId.HasValue);

        BoardingSummaryIsVisible = boarding != null;
        NoBoardingLabelIsVisible = boarding == null;

        if (boarding != null)
        {
            BoardingFlight = boarding.FlightNumber;
            BoardingSeat = string.IsNullOrWhiteSpace(boarding.SeatCode)
                ? "—" : boarding.SeatCode;
        }

        StatusLabel = "Dados atualizados a partir do servidor.";
    }

    private void SetFlight1(FlightDto? flight)
    {
        if (flight == null) { ClearFlight1(); return; }
        FlightOneNumber = flight.FlightNumber;
        FlightOneDestination = $"Para {flight.DestinationCode ?? flight.Destination}";
        FlightOneTime = $"{flight.DepartureTime:HH:mm} · {flight.Status}";
        FlightOneGate = string.IsNullOrWhiteSpace(flight.Gate) ? "—" : flight.Gate;
    }

    private void SetFlight2(FlightDto? flight)
    {
        if (flight == null) { ClearFlight2(); return; }
        FlightTwoNumber = flight.FlightNumber;
        FlightTwoDestination = $"Para {flight.DestinationCode ?? flight.Destination}";
        FlightTwoTime = $"{flight.DepartureTime:HH:mm} · {flight.Status}";
        FlightTwoGate = string.IsNullOrWhiteSpace(flight.Gate) ? "—" : flight.Gate;
    }

    private void ClearFlight1()
    {
        FlightOneNumber = "—";
        FlightOneDestination = "Sem dados";
        FlightOneTime = "—";
        FlightOneGate = "—";
    }

    private void ClearFlight2()
    {
        FlightTwoNumber = "—";
        FlightTwoDestination = "Sem dados";
        FlightTwoTime = "—";
        FlightTwoGate = "—";
    }
}
