using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusLabel = "A carregar a tua viagem…";

    // ─────────────────────────────────────────────
    // PRÓXIMOS VOOS
    // ─────────────────────────────────────────────

    [ObservableProperty]
    private bool _hasUpcomingFlights;

    [ObservableProperty]
    private string _flightOneNumber = string.Empty;

    [ObservableProperty]
    private string _flightOneOrigin = string.Empty;

    [ObservableProperty]
    private string _flightOneDestination = string.Empty;

    [ObservableProperty]
    private string _flightOneDate = string.Empty;

    [ObservableProperty]
    private string _flightOneTime = string.Empty;

    [ObservableProperty]
    private string _flightOneGate = string.Empty;

    // ─────────────────────────────────────────────
    // BOARDING PASS
    // ─────────────────────────────────────────────

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

    // ─────────────────────────────────────────────
    // CARREGAR HOME
    // ─────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            StatusLabel = "A carregar a tua viagem…";

            var departuresTask =
                _apiService.GetDeparturesAsync();

            var ticketsTask =
                _apiService.GetMyTicketsAsync();

            await Task.WhenAll(
                departuresTask,
                ticketsTask);

            var departures =
                await departuresTask;

            var tickets =
                await ticketsTask;

            Debug.WriteLine(
                $"[HomeViewModel] Departures: " +
                $"Success={departures.Success}, " +
                $"Count={departures.Data?.Count ?? 0}");

            Debug.WriteLine(
                $"[HomeViewModel] Tickets: " +
                $"Success={tickets.Success}, " +
                $"Count={tickets.Data?.Count ?? 0}");

            // ─────────────────────────────────────
            // PRÓXIMO VOO
            // ─────────────────────────────────────

            var now = DateTime.Now;

            var nextFlight =
                (departures.Data ?? new List<FlightDto>())
                .Where(f =>
                    f.DepartureTime > now &&
                    !string.Equals(
                        f.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        f.Status,
                        "Departed",
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.DepartureTime)
                .FirstOrDefault();

            SetUpcomingFlight(nextFlight);

            // ─────────────────────────────────────
            // BOARDING PASS
            // ─────────────────────────────────────

            var boarding =
                (tickets.Data ?? new List<TicketDto>())
                .Where(t =>
                    string.Equals(
                        t.Status,
                        "CheckedIn",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    t.BoardingPassId.HasValue)
                .OrderByDescending(t => t.Id)
                .FirstOrDefault();

            BoardingSummaryIsVisible =
                boarding != null;

            NoBoardingLabelIsVisible =
                boarding == null;

            if (boarding != null)
            {
                BoardingFlight =
                    boarding.FlightNumber;

                BoardingSeat =
                    string.IsNullOrWhiteSpace(
                        boarding.SeatCode)
                    ? "—"
                    : boarding.SeatCode;
            }
            else
            {
                BoardingFlight = "—";
                BoardingSeat = "—";
            }

            if (!departures.Success &&
                !tickets.Success)
            {
                StatusLabel =
                    "Não foi possível atualizar os dados.";
            }
            else
            {
                StatusLabel =
                    "Tudo pronto para a tua próxima viagem.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[HomeViewModel] Erro: {ex}");

            StatusLabel =
                "Não foi possível carregar os dados.";

            HasUpcomingFlights = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetUpcomingFlight(
        FlightDto? flight)
    {
        if (flight == null)
        {
            HasUpcomingFlights = false;

            FlightOneNumber = string.Empty;
            FlightOneOrigin = string.Empty;
            FlightOneDestination = string.Empty;
            FlightOneDate = string.Empty;
            FlightOneTime = string.Empty;
            FlightOneGate = string.Empty;

            return;
        }

        HasUpcomingFlights = true;

        FlightOneNumber =
            flight.FlightNumber;

        FlightOneOrigin =
      string.IsNullOrWhiteSpace(flight.Origin)
          ? "Origem"
          : flight.Origin;

        FlightOneDestination =
            flight.DestinationCode
            ?? flight.Destination
            ?? "Destino";

        FlightOneDate =
            flight.DepartureTime
                .ToString("dd MMM");

        FlightOneTime =
            flight.DepartureTime
                .ToString("HH:mm");

        FlightOneGate =
            string.IsNullOrWhiteSpace(flight.Gate)
                ? "Por definir"
                : flight.Gate;
    }
}