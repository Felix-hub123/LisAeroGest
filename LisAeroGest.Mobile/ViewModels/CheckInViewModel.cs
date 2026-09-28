using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Services;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels
{
    [QueryProperty(nameof(TicketId), "ticketId")]
    public partial class CheckInViewModel : ObservableObject
    {
        private readonly ApiService _apiService;


        // =========================================================
        // BILHETE
        // =========================================================

        [ObservableProperty]
        private int _ticketId;


        // =========================================================
        // ESTADO
        // =========================================================

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _checkInDone;


        // =========================================================
        // ERRO
        // =========================================================

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;


        // =========================================================
        // SUCESSO
        // =========================================================

        [ObservableProperty]
        private string _successMessage = string.Empty;

        [ObservableProperty]
        private bool _hasSuccess;


        // =========================================================
        // CARTÃO DE EMBARQUE
        // =========================================================

        [ObservableProperty]
        private string _flightNumber = string.Empty;

        [ObservableProperty]
        private string _gate = string.Empty;

        [ObservableProperty]
        private int _sequenceNumber;


        // =========================================================
        // PROPRIEDADES AUXILIARES
        // =========================================================

        public bool NotCheckedIn =>
            !CheckInDone;


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public CheckInViewModel(
            ApiService apiService)
        {
            _apiService = apiService;
        }


        // =========================================================
        // RECEBER TICKET ID
        // =========================================================

        partial void OnTicketIdChanged(
            int value)
        {
            if (value <= 0)
                return;

            MainThread.BeginInvokeOnMainThread(
                async () =>
                {
                    await LoadBoardingPassAsync();
                });
        }


        // =========================================================
        // ALTERAÇÃO DO ESTADO DO CHECK-IN
        // =========================================================

        partial void OnCheckInDoneChanged(
            bool value)
        {
            OnPropertyChanged(
                nameof(NotCheckedIn));
        }


        // =========================================================
        // LIMPAR FEEDBACK
        // =========================================================

        private void ClearFeedback()
        {
            HasError = false;
            ErrorMessage = string.Empty;

            HasSuccess = false;
            SuccessMessage = string.Empty;
        }


        // =========================================================
        // MOSTRAR ERRO
        // =========================================================

        private void ShowError(
            string message)
        {
            HasSuccess = false;
            SuccessMessage = string.Empty;

            ErrorMessage = message;
            HasError = true;
        }


        // =========================================================
        // MOSTRAR SUCESSO
        // =========================================================

        private void ShowSuccess(
            string message)
        {
            HasError = false;
            ErrorMessage = string.Empty;

            SuccessMessage = message;
            HasSuccess = true;
        }


        // =========================================================
        // CARREGAR CARTÃO DE EMBARQUE
        // =========================================================

        private async Task LoadBoardingPassAsync()
        {
            if (TicketId <= 0 ||
                IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;

                ClearFeedback();

                var result =
                    await _apiService
                        .GetBoardingPassAsync(
                            TicketId);


                // =================================================
                // CHECK-IN JÁ REALIZADO
                // =================================================

                if (result.Success &&
                    result.Data != null)
                {
                    FlightNumber =
                        result.Data.FlightNumber;

                    Gate =
                        string.IsNullOrWhiteSpace(
                            result.Data.Gate)
                            ? "Por atribuir"
                            : result.Data.Gate;

                    SequenceNumber =
                        result.Data.SequenceNumber;

                    CheckInDone = true;

                    Debug.WriteLine(
                        $"[CheckIn] Cartão encontrado " +
                        $"para Ticket {TicketId}.");

                    return;
                }


                // =================================================
                // CHECK-IN AINDA NÃO REALIZADO
                // =================================================

                CheckInDone = false;

                FlightNumber = string.Empty;
                Gate = string.Empty;
                SequenceNumber = 0;

                Debug.WriteLine(
                    $"[CheckIn] Cartão ainda não disponível " +
                    $"para Ticket {TicketId}. " +
                    $"Resposta: {result.ErrorMessage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[CheckIn] Erro ao carregar cartão: {ex}");

                CheckInDone = false;

                ShowError(
                    "Não foi possível consultar o estado do check-in.");
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // REALIZAR CHECK-IN
        // =========================================================

        [RelayCommand]
        public async Task ConfirmCheckInAsync()
        {
            if (IsBusy)
                return;


            // =====================================================
            // VALIDAR BILHETE
            // =====================================================

            if (TicketId <= 0)
            {
                ShowError(
                    "Bilhete inválido.");

                return;
            }


            // =====================================================
            // CONFIRMAR OPERAÇÃO
            // =====================================================

            var confirmed =
                await Shell.Current.DisplayAlert(
                    "Confirmar check-in",
                    "Confirmas que pretendes realizar o check-in para este voo?",
                    "Fazer check-in",
                    "Cancelar");

            if (!confirmed)
                return;


            // =====================================================
            // FAZER CHECK-IN
            // =====================================================

            try
            {
                IsBusy = true;

                ClearFeedback();

                var result =
                    await _apiService
                        .DoCheckInAsync(
                            TicketId);


                // =================================================
                // ERRO DA API
                // =================================================

                if (!result.Success)
                {
                    ShowError(
                        result.ErrorMessage ??
                        "Não foi possível fazer o check-in.");

                    return;
                }


                // =================================================
                // ATUALIZAR CARTÃO
                // =================================================

                FlightNumber =
                    result.FlightNumber;

                Gate =
                    string.IsNullOrWhiteSpace(
                        result.Gate)
                        ? "Por atribuir"
                        : result.Gate;

                SequenceNumber =
                    result.SequenceNumber;

                CheckInDone = true;


                // =================================================
                // FEEDBACK
                // =================================================

                ShowSuccess(
                    $"Check-in realizado com sucesso para o voo " +
                    $"{FlightNumber}.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[CheckIn] Erro no check-in: {ex}");

                ShowError(
                    "Ocorreu um erro de ligação ao servidor.");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}