using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.Views;

public partial class OperationsPassengersPage : ContentPage
{
    private readonly ApiService _apiService;

    public OperationsPassengersPage() : this(ResolveApiService()) { }

    public OperationsPassengersPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private static ApiService ResolveApiService()
        => Application.Current?.Handler?.MauiContext?.Services.GetService<ApiService>()
           ?? throw new InvalidOperationException("ApiService não está registado.");

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        var query = SearchEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            await DisplayAlert("Pesquisa", "Introduz o nome, email, documento, voo ou número do bilhete.", "OK");
            return;
        }

        SetBusy(true);
        var result = await _apiService.SearchEmployeeTicketsAsync(query);
        SetBusy(false);

        if (!result.Success)
        {
            StatusLabel.Text = result.ErrorMessage ?? "Não foi possível pesquisar os passageiros.";
            ResultsView.ItemsSource = Array.Empty<EmployeeCheckInDto>();
            return;
        }

        var tickets = result.Data ?? new List<EmployeeCheckInDto>();
        ResultsView.ItemsSource = tickets;
        StatusLabel.Text = tickets.Count == 0 ? "Nenhum passageiro encontrado." : $"{tickets.Count} resultado(s) encontrado(s).";
    }

    private async void OnCheckInClicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not EmployeeCheckInDto ticket)
            return;

        var confirm = await DisplayAlert("Confirmar check-in", $"Confirmar o check-in de {ticket.PassengerName} no voo {ticket.FlightNumber}?", "Confirmar", "Cancelar");
        if (!confirm) return;

        SetBusy(true);
        var result = await _apiService.EmployeeCheckInAsync(ticket.TicketId);
        SetBusy(false);

        if (!result.Success)
        {
            await DisplayAlert("Check-in não concluído", result.ErrorMessage ?? "Não foi possível concluir o check-in.", "OK");
            return;
        }

        await DisplayAlert("Check-in concluído", $"{result.PassengerName ?? ticket.PassengerName}\nVoo: {result.FlightNumber}\nPorta: {result.Gate}\nSequência: {result.SequenceNumber}", "OK");
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        var query = SearchEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query)) return;
        SetBusy(true);
        var result = await _apiService.SearchEmployeeTicketsAsync(query);
        SetBusy(false);
        ResultsView.ItemsSource = result.Data ?? new List<EmployeeCheckInDto>();
    }

    private void SetBusy(bool busy)
    {
        BusyIndicator.IsVisible = busy;
        BusyIndicator.IsRunning = busy;
    }
}
