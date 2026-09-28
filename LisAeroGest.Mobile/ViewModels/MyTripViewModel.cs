using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class MyTripViewModel : ObservableObject
    {
        private readonly ApiService _apiService;


        // =========================================================
        // ESTADO
        // =========================================================

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;


        // =========================================================
        // VIAGEM
        // =========================================================

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


        // =========================================================
        // VISIBILIDADE
        // =========================================================

        [ObservableProperty]
        private bool _tripCardIsVisible;

        [ObservableProperty]
        private bool _timelineSectionIsVisible;

        [ObservableProperty]
        private bool _emptyCardIsVisible;

        [ObservableProperty]
        private bool _checkInButtonIsVisible;


        // =========================================================
        // CHECK-IN
        // =========================================================

        [ObservableProperty]
        private string _checkInIconLabel = "→";

        [ObservableProperty]
        private string _checkInTitle = "Check-in disponível";

        [ObservableProperty]
        private string _checkInSubtitle =
            "Confirma o teu check-in antes da partida.";


        // =========================================================
        // BILHETE ATUAL
        // =========================================================

        public TicketDto? CurrentTrip
        {
            get;
            private set;
        }


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public MyTripViewModel(
            ApiService apiService)
        {
            _apiService = apiService;
        }


        // =========================================================
        // CARREGAR VIAGEM
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


                // =================================================
                // OBTER BILHETES
                // =================================================

                var result =
                    await _apiService
                        .GetMyTicketsAsync();


                if (!result.Success)
                {
                    ShowEmpty();

                    ShowError(
                        result.ErrorMessage ??
                        "Não foi possível carregar a tua viagem.");

                    return;
                }


                // =================================================
                // PRÓXIMA VIAGEM
                // =================================================

                CurrentTrip =
                    (result.Data ?? new List<TicketDto>())
                    .Where(ticket =>
                        ticket.DepartureTime >= DateTime.Now)
                    .OrderBy(ticket =>
                        ticket.DepartureTime)
                    .FirstOrDefault();


                // =================================================
                // SEM VIAGEM
                // =================================================

                if (CurrentTrip == null)
                {
                    ShowEmpty();
                    return;
                }


                // =================================================
                // DADOS DO VOO
                // =================================================

                TripFlightNumber =
                    CurrentTrip.FlightNumber;

                TripRoute =
                    $"{CurrentTrip.Origin} → {CurrentTrip.Destination}";

                TripStatus =
                    FormatStatus(
                        CurrentTrip.Status);

                TripDeparture =
                    CurrentTrip.DepartureTime
                        .ToString("dd/MM/yyyy HH:mm");


                // =================================================
                // LUGAR
                // =================================================

                if (string.IsNullOrWhiteSpace(
                        CurrentTrip.SeatCode))
                {
                    TripSeat = "—";

                    TripSeatDetail =
                        "Sem lugar atribuído";
                }
                else
                {
                    TripSeat =
                        CurrentTrip.SeatCode;

                    TripSeatDetail =
                        $"Lugar {CurrentTrip.SeatCode} atribuído";
                }


                // =================================================
                // CHECK-IN
                // =================================================

                var isCheckedIn =
                    string.Equals(
                        CurrentTrip.Status,
                        "CheckedIn",
                        StringComparison.OrdinalIgnoreCase);


                if (isCheckedIn)
                {
                    CheckInIconLabel = "✓";

                    CheckInTitle =
                        "Check-in confirmado";

                    CheckInSubtitle =
                        "O teu check-in está concluído. Consulta os dados de embarque na carteira.";

                    CheckInButtonIsVisible =
                        false;
                }
                else
                {
                    CheckInIconLabel = "→";

                    CheckInTitle =
                        "Check-in disponível";

                    CheckInSubtitle =
                        "Confirma o teu check-in antes da partida.";

                    CheckInButtonIsVisible =
                        true;
                }


                // =================================================
                // MOSTRAR VIAGEM
                // =================================================

                TripCardIsVisible = true;
                TimelineSectionIsVisible = true;
                EmptyCardIsVisible = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[MyTripViewModel] {ex}");

                ShowEmpty();

                ShowError(
                    "Não foi possível carregar a tua viagem. Verifica a ligação e tenta novamente.");
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // ESTADO VAZIO
        // =========================================================

        private void ShowEmpty()
        {
            TripCardIsVisible = false;
            TimelineSectionIsVisible = false;
            CheckInButtonIsVisible = false;
            EmptyCardIsVisible = true;

            CurrentTrip = null;
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


        // =========================================================
        // FORMATAR ESTADO
        // =========================================================

        private static string FormatStatus(
            string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "CONFIRMADO";


            return status.ToLowerInvariant() switch
            {
                "checkedin" => "CHECK-IN CONCLUÍDO",
                "confirmed" => "CONFIRMADO",
                "paid" => "PAGO",
                "pending" => "PENDENTE",
                "cancelled" => "CANCELADO",
                _ => status.ToUpperInvariant()
            };
        }
    }
}