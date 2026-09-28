using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels;

public partial class OperationsCommunicationsViewModel : ObservableObject
{
    private readonly ApiService _apiService;


    // =========================================================
    // VOO
    // =========================================================

    [ObservableProperty]
    private int _flightId;

    [ObservableProperty]
    private FlightDto? _selectedFlight;


    // =========================================================
    // COMUNICAÇÃO
    // =========================================================

    [ObservableProperty]
    private string _selectedType = "Informação";

    [ObservableProperty]
    private string _message = string.Empty;


    // =========================================================
    // ESTADO
    // =========================================================

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private bool _hasSuccess;

    [ObservableProperty]
    private string _statusLabel = string.Empty;


    // =========================================================
    // LISTAS
    // =========================================================

    public ObservableCollection<FlightDto> Flights { get; } = new();


    public List<string> CommunicationTypes { get; } = new()
    {
        "Informação",
        "Embarque",
        "Porta",
        "Atraso"
    };


    // =========================================================
    // PROPRIEDADES CALCULADAS
    // =========================================================

    public bool HasSelectedFlight =>
        SelectedFlight != null;


    public string SelectedFlightTitle =>
        SelectedFlight == null
            ? "Selecione um voo"
            : $"{SelectedFlight.FlightNumber} · " +
              $"{SelectedFlight.Origin} → " +
              $"{SelectedFlight.Destination}";


    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public OperationsCommunicationsViewModel(
        ApiService apiService)
    {
        _apiService = apiService;
    }


    // =========================================================
    // VOO SELECIONADO
    // =========================================================

    partial void OnSelectedFlightChanged(
        FlightDto? value)
    {
        OnPropertyChanged(
            nameof(HasSelectedFlight));

        OnPropertyChanged(
            nameof(SelectedFlightTitle));

        ClearFeedback();
    }


    // =========================================================
    // CARREGAR VOOS
    // =========================================================

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            ClearFeedback();

            StatusLabel = string.Empty;


            var result =
                await _apiService
                    .GetDeparturesAsync();


            if (!result.Success)
            {
                Flights.Clear();

                ShowError(
                    result.ErrorMessage ??
                    "Não foi possível carregar os voos.");

                return;
            }


            var flights =
                (result.Data ?? new List<FlightDto>())
                .Where(f =>
                    !string.Equals(
                        f.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(f =>
                    f.DepartureTime)
                .ToList();


            Flights.Clear();


            foreach (var flight in flights)
            {
                Flights.Add(flight);
            }


            // =================================================
            // VOO RECEBIDO ATRAVÉS DA NAVEGAÇÃO
            // =================================================

            if (FlightId > 0)
            {
                SelectedFlight =
                    Flights.FirstOrDefault(
                        f => f.Id == FlightId);
            }


            // =================================================
            // APENAS UM VOO
            // =================================================

            if (SelectedFlight == null &&
                Flights.Count == 1)
            {
                SelectedFlight =
                    Flights[0];
            }


            // =================================================
            // SEM VOOS
            // =================================================

            if (Flights.Count == 0)
            {
                StatusLabel =
                    "Não existem voos disponíveis.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[OperationsCommunicationsViewModel] Load: {ex}");

            Flights.Clear();

            ShowError(
                "Ocorreu um erro ao carregar os voos.");
        }
        finally
        {
            IsBusy = false;
        }
    }


    // =========================================================
    // ENVIAR COMUNICAÇÃO
    // =========================================================

    [RelayCommand]
    private async Task SendAsync()
    {
        if (IsBusy)
            return;


        ClearFeedback();
        StatusLabel = string.Empty;


        // =====================================================
        // VALIDAR VOO
        // =====================================================

        if (SelectedFlight == null)
        {
            ShowError(
                "Selecione primeiro um voo.");

            return;
        }


        // =====================================================
        // VALIDAR MENSAGEM
        // =====================================================

        if (string.IsNullOrWhiteSpace(Message))
        {
            ShowError(
                "Escreva a mensagem.");

            return;
        }


        if (Message.Trim().Length > 500)
        {
            ShowError(
                "A mensagem não pode exceder 500 caracteres.");

            return;
        }


        // =====================================================
        // CONFIRMAÇÃO
        // =====================================================

        var confirmed =
            await Shell.Current.DisplayAlert(
                "Enviar comunicação",
                $"Enviar esta comunicação aos passageiros " +
                $"do voo {SelectedFlight.FlightNumber}?",
                "Enviar",
                "Cancelar");


        if (!confirmed)
            return;


        try
        {
            IsBusy = true;

            ClearFeedback();


            // Guardamos o número do voo antes do pedido.

            var flightNumber =
                SelectedFlight.FlightNumber;


            var request =
                new FlightCommunicationRequestDto
                {
                    Type =
                        ConvertType(
                            SelectedType),

                    Message =
                        Message.Trim()
                };


            var result =
                await _apiService
                    .SendFlightCommunicationAsync(
                        SelectedFlight.Id,
                        request);


            // =================================================
            // ERRO
            // =================================================

            if (!result.Success ||
                result.Data == null)
            {
                ShowError(
                    result.ErrorMessage ??
                    "Não foi possível enviar a comunicação.");

                return;
            }


            // =================================================
            // SUCESSO
            // =================================================

            ShowSuccess(
                $"Comunicação enviada aos passageiros do voo " +
                $"{flightNumber}. " +
                $"{result.Data.Recipients} passageiro(s) notificado(s).");


            // Limpar apenas a mensagem.
            // Mantemos voo e tipo selecionados para facilitar
            // uma nova comunicação para o mesmo voo.

            Message = string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[OperationsCommunicationsViewModel] Send: {ex}");

            ShowError(
                "Ocorreu um erro ao enviar a comunicação.");
        }
        finally
        {
            IsBusy = false;
        }
    }


    // =========================================================
    // TIPO DE COMUNICAÇÃO
    // =========================================================

    private static string ConvertType(
        string type)
    {
        return type switch
        {
            "Embarque" => "Boarding",
            "Porta" => "Gate",
            "Atraso" => "Delay",
            _ => "Info"
        };
    }


    // =========================================================
    // FEEDBACK
    // =========================================================

    private void ClearFeedback()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        HasSuccess = false;
        SuccessMessage = string.Empty;
    }


    private void ShowError(
        string message)
    {
        HasSuccess = false;
        SuccessMessage = string.Empty;

        ErrorMessage = message;
        HasError = true;
    }


    private void ShowSuccess(
        string message)
    {
        HasError = false;
        ErrorMessage = string.Empty;

        SuccessMessage = message;
        HasSuccess = true;
    }
}