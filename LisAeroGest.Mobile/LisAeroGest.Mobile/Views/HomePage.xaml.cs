using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.Views;

public partial class HomePage : ContentPage
{
    private readonly ApiService _apiService;

    public HomePage() : this(ResolveApiService()) { }

    public HomePage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private static ApiService ResolveApiService()
        => Application.Current?.Handler?.MauiContext?.Services.GetService<ApiService>()
           ?? throw new InvalidOperationException("ApiService não está registado.");

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        DateLabel.Text = DateTime.Now.ToString("dd MMM yyyy");
        await LoadAsync();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadAsync();
        Refresh.IsRefreshing = false;
    }

    private async Task LoadAsync()
    {
        StatusLabel.Text = "A carregar dados da tua conta…";
        var departuresTask = _apiService.GetDeparturesAsync();
        var ticketsTask = _apiService.GetMyTicketsAsync();
        await Task.WhenAll(departuresTask, ticketsTask);

        var departures = departuresTask.Result;
        var tickets = ticketsTask.Result;
        if (!departures.Success && !tickets.Success)
        {
            StatusLabel.Text = departures.ErrorMessage ?? tickets.ErrorMessage ?? "Não foi possível carregar os dados.";
            ClearFlight(FlightOneNumber, FlightOneDestination, FlightOneTime, FlightOneGate);
            ClearFlight(FlightTwoNumber, FlightTwoDestination, FlightTwoTime, FlightTwoGate);
            BoardingSummary.IsVisible = false;
            NoBoardingLabel.IsVisible = true;
            return;
        }

        var flights = (departures.Data ?? new List<FlightDto>()).OrderBy(f => f.DepartureTime).Take(2).ToList();
        SetFlight(flights.ElementAtOrDefault(0), FlightOneNumber, FlightOneDestination, FlightOneTime, FlightOneGate);
        SetFlight(flights.ElementAtOrDefault(1), FlightTwoNumber, FlightTwoDestination, FlightTwoTime, FlightTwoGate);

        var boarding = (tickets.Data ?? new List<TicketDto>()).FirstOrDefault(t => t.Status == "CheckedIn" || t.BoardingPassId.HasValue);
        BoardingSummary.IsVisible = boarding != null;
        NoBoardingLabel.IsVisible = boarding == null;
        if (boarding != null)
        {
            BoardingFlight.Text = boarding.FlightNumber;
            BoardingSeat.Text = string.IsNullOrWhiteSpace(boarding.SeatCode) ? "—" : boarding.SeatCode;
        }

        StatusLabel.Text = "Dados atualizados a partir do servidor.";
    }

    private static void SetFlight(FlightDto? flight, Label number, Label destination, Label time, Label gate)
    {
        if (flight == null)
        {
            ClearFlight(number, destination, time, gate);
            return;
        }

        number.Text = flight.FlightNumber;
        destination.Text = $"Para {flight.DestinationCode ?? flight.Destination}";
        time.Text = $"{flight.DepartureTime:HH:mm} · {flight.Status}";
        gate.Text = string.IsNullOrWhiteSpace(flight.Gate) ? "—" : flight.Gate;
    }

    private static void ClearFlight(Label number, Label destination, Label time, Label gate)
    {
        number.Text = "—";
        destination.Text = "Sem dados";
        time.Text = "—";
        gate.Text = "—";
    }

    private async void OnVerBilhetesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//TicketsPage");
    }
}
