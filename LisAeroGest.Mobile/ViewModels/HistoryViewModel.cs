using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
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
        // HISTÓRICO
        // =========================================================

        public ObservableCollection<TicketDto> Tickets
        {
            get;
        } = new();


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public HistoryViewModel(
            ApiService apiService)
        {
            _apiService = apiService;
        }


        // =========================================================
        // CARREGAR HISTÓRICO
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
                // OBTER BILHETES DO PASSAGEIRO
                // =================================================

                var result =
                    await _apiService
                        .GetMyTicketsAsync();


                Tickets.Clear();


                // =================================================
                // ERRO DA API
                // =================================================

                if (!result.Success)
                {
                    ShowError(
                        result.ErrorMessage ??
                        "Não foi possível carregar o histórico.");

                    return;
                }


                // =================================================
                // APENAS VIAGENS ANTERIORES
                // =================================================

                var previousTrips =
                    (result.Data ?? new List<TicketDto>())
                    .Where(ticket =>
                        ticket.DepartureTime < DateTime.Now)
                    .OrderByDescending(ticket =>
                        ticket.DepartureTime)
                    .ToList();


                foreach (var ticket in previousTrips)
                {
                    Tickets.Add(ticket);
                }
            }
            catch (Exception ex)
            {
                Tickets.Clear();

                Debug.WriteLine(
                    $"[HistoryViewModel] {ex}");

                ShowError(
                    "Não foi possível carregar o histórico. Verifica a ligação e tenta novamente.");
            }
            finally
            {
                IsBusy = false;
            }
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