using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.Views;

public partial class OperationsPassengersPage : ContentPage
{
    private readonly ApiService _apiService;

    public OperationsPassengersPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        await SearchAsync();
    }

    private async void OnSearchReturn(object sender, EventArgs e)
    {
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        var query = SearchEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            await DisplayAlert(
                "Pesquisa",
                "Introduz o nome do passageiro.",
                "OK");
            return;
        }

        SetBusy(true);

        try
        {
            var result = await _apiService.SearchEmployeeTicketsAsync(query);

            if (!result.Success)
            {
                StatusLabel.Text = result.ErrorMessage
                    ?? "Não foi possível pesquisar os passageiros.";
                ResultsView.ItemsSource = Array.Empty<EmployeeCheckInDto>();
                return;
            }

            var tickets = result.Data ?? new List<EmployeeCheckInDto>();
            ResultsView.ItemsSource = tickets;

            StatusLabel.Text = tickets.Count == 0
                ? "Nenhum passageiro encontrado."
                : $"{tickets.Count} resultado(s) encontrado(s).";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Erro ao pesquisar. Tenta novamente.";
            System.Diagnostics.Debug.WriteLine(
                $"[OperationsPassengersPage] {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnCheckInClicked(object sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.BindingContext is not EmployeeCheckInDto ticket)
            return;

        var confirm = await DisplayAlert(
            "Confirmar check-in",
            $"Confirmar o check-in de {ticket.PassengerName} no voo {ticket.FlightNumber}?",
            "Confirmar",
            "Cancelar");

        if (!confirm)
            return;

        SetBusy(true);

        try
        {
            var result = await _apiService.EmployeeCheckInByNameAsync(
                ticket.PassengerName,
                ticket.FlightNumber);

            if (!result.Success)
            {
                await DisplayAlert(
                    "Check-in não concluído",
                    result.ErrorMessage ?? "Não foi possível concluir o check-in.",
                    "OK");
                return;
            }

            await DisplayAlert(
                "Check-in concluído",
                $"{result.PassengerName ?? ticket.PassengerName}\n" +
                $"Voo: {result.FlightNumber}\n" +
                $"Porta: {result.Gate}\n" +
                $"Sequência: {result.SequenceNumber}",
                "OK");

            // Recarrega a lista para refletir o novo estado
            await SearchAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Erro",
                "Ocorreu um erro ao concluir o check-in.",
                "OK");

            System.Diagnostics.Debug.WriteLine(
                $"[OperationsPassengersPage] Check-in: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        BusyIndicator.IsVisible = busy;
        BusyIndicator.IsRunning = busy;
    }
}