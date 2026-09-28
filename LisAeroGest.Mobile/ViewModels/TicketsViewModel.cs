using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class TicketsViewModel : ObservableObject
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
        // BILHETES
        // =========================================================

        public ObservableCollection<TicketDto> Tickets
        {
            get;
        } = new();


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public TicketsViewModel(
            ApiService apiService)
        {
            _apiService = apiService;
        }


        // =========================================================
        // CARREGAR BILHETES
        // =========================================================

        [RelayCommand]
        public async Task LoadTicketsAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                HasError = false;
                ErrorMessage = string.Empty;


                var result =
                    await _apiService
                        .GetMyTicketsAsync();


                Tickets.Clear();


                // =================================================
                // ERRO
                // =================================================

                if (result == null ||
                    !result.Success)
                {
                    ShowError(
                        result?.ErrorMessage ??
                        "Não foi possível carregar os bilhetes.");

                    return;
                }


                // =================================================
                // ORDENAR BILHETES
                //
                // Próximas viagens aparecem primeiro.
                // Depois aparecem as viagens anteriores.
                // =================================================

                var now = DateTime.Now;

                var tickets =
                    (result.Data ?? new List<TicketDto>())
                    .OrderBy(ticket =>
                        ticket.DepartureTime < now)
                    .ThenBy(ticket =>
                        ticket.DepartureTime < now
                            ? DateTime.MaxValue
                            : ticket.DepartureTime)
                    .ThenByDescending(ticket =>
                        ticket.DepartureTime < now
                            ? ticket.DepartureTime
                            : DateTime.MinValue)
                    .ToList();


                foreach (var ticket in tickets)
                {
                    Tickets.Add(ticket);
                }
            }
            catch (Exception ex)
            {
                Tickets.Clear();

                Debug.WriteLine(
                    $"[TicketsViewModel] {ex}");

                ShowError(
                    "Não foi possível carregar os bilhetes. Verifica a ligação e tenta novamente.");
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // ABRIR BILHETE
        // =========================================================

        [RelayCommand]
        private async Task OpenTicketAsync(
            TicketDto? ticket)
        {
            if (ticket == null ||
                IsBusy)
            {
                return;
            }


            // =====================================================
            // CHECK-IN CONCLUÍDO
            // =====================================================

            if (string.Equals(
                    ticket.Status,
                    "CheckedIn",
                    StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.GoToAsync(
                    nameof(CheckInPage),
                    new Dictionary<string, object>
                    {
                        ["ticketId"] = ticket.Id
                    });

                return;
            }


            // =====================================================
            // BILHETE PAGO
            // =====================================================

            if (string.Equals(
                    ticket.Status,
                    "Paid",
                    StringComparison.OrdinalIgnoreCase))
            {
                await Shell.Current.GoToAsync(
                    nameof(CheckInPage),
                    new Dictionary<string, object>
                    {
                        ["ticketId"] = ticket.Id
                    });

                return;
            }


            // =====================================================
            // OUTROS ESTADOS
            // =====================================================

            await Shell.Current.DisplayAlert(
                "Bilhete",
                $"Estado atual: {FormatStatus(ticket.Status)}.",
                "OK");
        }


        // =========================================================
        // FORMATAR ESTADO
        // =========================================================

        private static string FormatStatus(
            string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "Desconhecido";


            return status.ToLowerInvariant() switch
            {
                "paid" => "Pago",
                "checkedin" => "Check-in concluído",
                "confirmed" => "Confirmado",
                "pending" => "Pendente",
                "cancelled" => "Cancelado",
                _ => status
            };
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
}