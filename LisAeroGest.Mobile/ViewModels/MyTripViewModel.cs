using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.ViewModels;

public partial class MyTripViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string _statusLabel = "A carregar a tua viagem…";

    // ── Trip Card ─────────────────────────────────────────────────────────────
    [ObservableProperty]
    private string _tripFlightNumber = "—";

    [ObservableProperty]
    private string _tripRoute = "—";

    [ObservableProperty]
    private string _tripStatus = "—";

    [ObservableProperty]
    private string _tripDeparture = "—";

    [ObservableProperty]
    private string _tripSeat = "—";

    [ObservableProperty]
    private string _tripSeatDetail = "Sem lugar atribuído";

    [ObservableProperty]
    private bool _tripCardIsVisible;

    [ObservableProperty]
    private bool _timelineSectionIsVisible;

    [ObservableProperty]
    private bool _emptyCardIsVisible;

    [ObservableProperty]
    private bool _checkInButtonIsVisible;

    [ObservableProperty]
    private string _checkInIconLabel = "→";

    [ObservableProperty]
    private string _checkInTitle = "Check-in disponível";

    [ObservableProperty]
    private string _checkInSubtitle = "Gera o teu cartão de embarque antes da partida.";

    // Passageiro guardado para navegação
    public TicketDto? CurrentTrip { get; private set; }

    public MyTripViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        StatusLabel = "A carregar a tua viagem…";

        var result = await _apiService.GetMyTicketsAsync();

        if (!result.Success)
        {
            StatusLabel = result.ErrorMessage
                ?? "Não foi possível carregar a tua viagem.";
            ShowEmpty();
            return;
        }

        // A viagem é o bilhete com partida mais próxima no futuro
        CurrentTrip = (result.Data ?? new List<TicketDto>())
            .Where(t => t.DepartureTime >= DateTime.Now)
            .OrderBy(t => t.DepartureTime)
            .FirstOrDefault();

        if (CurrentTrip == null)
        {
            StatusLabel = "Ainda não tens nenhuma viagem agendada.";
            ShowEmpty();
            return;
        }

        var isCheckedIn = CurrentTrip.Status == "CheckedIn";

        TripCardIsVisible = true;
        TimelineSectionIsVisible = true;
        EmptyCardIsVisible = false;

        TripFlightNumber = CurrentTrip.FlightNumber;
        TripRoute = $"{CurrentTrip.Origin} → {CurrentTrip.Destination}";
        TripStatus = CurrentTrip.Status.ToUpperInvariant();
        TripDeparture = CurrentTrip.DepartureTime.ToString("dd/MM HH:mm");
        TripSeat = string.IsNullOrWhiteSpace(CurrentTrip.SeatCode)
            ? "—" : CurrentTrip.SeatCode;
        TripSeatDetail = string.IsNullOrWhiteSpace(CurrentTrip.SeatCode)
            ? "Sem lugar atribuído"
            : $"Lugar {CurrentTrip.SeatCode} atribuído";

        if (isCheckedIn)
        {
            CheckInIconLabel = "✓";
            CheckInTitle = "Check-in confirmado";
            CheckInSubtitle = "O teu cartão de embarque já está disponível na carteira.";
            CheckInButtonIsVisible = false;
        }
        else
        {
            CheckInIconLabel = "→";
            CheckInTitle = "Check-in disponível";
            CheckInSubtitle = "Gera o teu cartão de embarque antes da partida.";
            CheckInButtonIsVisible = true;
        }

        StatusLabel = "Dados atualizados a partir do servidor.";
    }

    private void ShowEmpty()
    {
        TripCardIsVisible = false;
        TimelineSectionIsVisible = false;
        CheckInButtonIsVisible = false;
        EmptyCardIsVisible = true;
        CurrentTrip = null;
    }
}
