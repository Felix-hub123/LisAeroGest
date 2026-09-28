using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ApiService _apiService;


    // =========================================================
    // ESTADO
    // =========================================================

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusLabel = "A carregar a tua viagem…";

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;


    // =========================================================
    // PRÓXIMA VIAGEM DO PASSAGEIRO
    // =========================================================

    [ObservableProperty]
    private bool _hasUpcomingFlights;

    [ObservableProperty]
    private bool _hasNoUpcomingFlights = true;

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


    // =========================================================
    // CARTÃO DE EMBARQUE
    // =========================================================

    [ObservableProperty]
    private string _boardingFlight = "—";

    [ObservableProperty]
    private string _boardingSeat = "—";

    [ObservableProperty]
    private bool _boardingSummaryIsVisible;

    [ObservableProperty]
    private bool _noBoardingLabelIsVisible = true;


    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public HomeViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }


    // =========================================================
    // CARREGAR HOME
    // =========================================================

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            HasError = false;
            ErrorMessage = string.Empty;

            StatusLabel = "A carregar a tua viagem…";


            // =================================================
            // CARREGAR VOOS + BILHETES
            // =================================================

            var departuresTask =
                _apiService.GetDeparturesAsync();

            var ticketsTask =
                _apiService.GetMyTicketsAsync();


            await Task.WhenAll(
                departuresTask,
                ticketsTask);


            var departuresResult =
                await departuresTask;

            var ticketsResult =
                await ticketsTask;


            Debug.WriteLine(
                $"[HomeViewModel] Departures: " +
                $"Success={departuresResult.Success}, " +
                $"Count={departuresResult.Data?.Count ?? 0}");

            Debug.WriteLine(
                $"[HomeViewModel] Tickets: " +
                $"Success={ticketsResult.Success}, " +
                $"Count={ticketsResult.Data?.Count ?? 0}");


            // =================================================
            // VERIFICAR BILHETES
            // =================================================

            if (!ticketsResult.Success)
            {
                ClearUpcomingFlight();
                ClearBoardingPass();

                ShowError(
                    ticketsResult.ErrorMessage ??
                    "Não foi possível carregar as tuas viagens.");

                StatusLabel =
                    "Não foi possível atualizar as tuas viagens.";

                return;
            }


            var tickets =
                ticketsResult.Data ??
                new List<TicketDto>();

            var now = DateTime.Now;


            // =================================================
            // PRÓXIMA VIAGEM DO PASSAGEIRO
            //
            // IMPORTANTE:
            // Não usamos simplesmente o próximo voo do aeroporto.
            // Primeiro procuramos o próximo BILHETE do passageiro.
            // =================================================

            var nextTicket =
                tickets
                    .Where(ticket =>
                        ticket.DepartureTime >= now &&
                        !string.Equals(
                            ticket.Status,
                            "Cancelled",
                            StringComparison.OrdinalIgnoreCase))
                    .OrderBy(ticket =>
                        ticket.DepartureTime)
                    .FirstOrDefault();


            if (nextTicket == null)
            {
                ClearUpcomingFlight();
            }
            else
            {
                // Procurar o mesmo voo na lista operacional
                // para obter informação atualizada, como a porta.

                FlightDto? matchingFlight = null;

                if (departuresResult.Success)
                {
                    matchingFlight =
                        (departuresResult.Data ??
                         new List<FlightDto>())
                        .FirstOrDefault(flight =>
                            string.Equals(
                                flight.FlightNumber,
                                nextTicket.FlightNumber,
                                StringComparison.OrdinalIgnoreCase));
                }


                SetUpcomingFlight(
                    nextTicket,
                    matchingFlight);
            }


            // =================================================
            // CARTÃO DE EMBARQUE
            //
            // Mostrar o próximo bilhete com check-in concluído.
            // =================================================

            var boarding =
                tickets
                    .Where(ticket =>
                        ticket.DepartureTime >= now &&
                        (
                            string.Equals(
                                ticket.Status,
                                "CheckedIn",
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            ticket.BoardingPassId.HasValue
                        ))
                    .OrderBy(ticket =>
                        ticket.DepartureTime)
                    .FirstOrDefault();


            SetBoardingPass(boarding);


            // =================================================
            // STATUS
            // =================================================

            if (nextTicket != null)
            {
                StatusLabel =
                    "Tudo pronto para a tua próxima viagem.";
            }
            else
            {
                StatusLabel =
                    "Ainda não tens nenhuma viagem agendada.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[HomeViewModel] Erro: {ex}");

            ClearUpcomingFlight();
            ClearBoardingPass();

            StatusLabel =
                "Não foi possível carregar os dados.";

            ShowError(
                "Não foi possível atualizar a página inicial. Verifica a ligação e tenta novamente.");
        }
        finally
        {
            IsBusy = false;
        }
    }


    // =========================================================
    // DEFINIR PRÓXIMA VIAGEM
    // =========================================================

    private void SetUpcomingFlight(
        TicketDto ticket,
        FlightDto? flight)
    {
        HasUpcomingFlights = true;
        HasNoUpcomingFlights = false;


        FlightOneNumber =
            ticket.FlightNumber;


        FlightOneOrigin =
            string.IsNullOrWhiteSpace(ticket.Origin)
                ? "Origem"
                : ticket.Origin;


        FlightOneDestination =
            string.IsNullOrWhiteSpace(ticket.Destination)
                ? "Destino"
                : ticket.Destination;


        FlightOneDate =
            ticket.DepartureTime
                .ToString("dd MMM");


        FlightOneTime =
            ticket.DepartureTime
                .ToString("HH:mm");


        // A porta vem da informação operacional do voo.
        // Se não estiver disponível, mostramos "Por definir".

        FlightOneGate =
            flight == null ||
            string.IsNullOrWhiteSpace(flight.Gate)
                ? "Por definir"
                : flight.Gate;
    }


    // =========================================================
    // LIMPAR PRÓXIMA VIAGEM
    // =========================================================

    private void ClearUpcomingFlight()
    {
        HasUpcomingFlights = false;
        HasNoUpcomingFlights = true;

        FlightOneNumber = string.Empty;
        FlightOneOrigin = string.Empty;
        FlightOneDestination = string.Empty;
        FlightOneDate = string.Empty;
        FlightOneTime = string.Empty;
        FlightOneGate = string.Empty;
    }


    // =========================================================
    // CARTÃO DE EMBARQUE
    // =========================================================

    private void SetBoardingPass(
        TicketDto? ticket)
    {
        if (ticket == null)
        {
            ClearBoardingPass();
            return;
        }


        BoardingSummaryIsVisible = true;
        NoBoardingLabelIsVisible = false;


        BoardingFlight =
            string.IsNullOrWhiteSpace(ticket.FlightNumber)
                ? "—"
                : ticket.FlightNumber;


        BoardingSeat =
            string.IsNullOrWhiteSpace(ticket.SeatCode)
                ? "—"
                : ticket.SeatCode;
    }


    // =========================================================
    // LIMPAR CARTÃO DE EMBARQUE
    // =========================================================

    private void ClearBoardingPass()
    {
        BoardingSummaryIsVisible = false;
        NoBoardingLabelIsVisible = true;

        BoardingFlight = "—";
        BoardingSeat = "—";
    }


    // =========================================================
    // ERRO
    // =========================================================

    private void ShowError(
        string message)
    {
        ErrorMessage = message;
        HasError = true;
    }
}