using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels;

public partial class OperationsGatesViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private int _flightId;

    [ObservableProperty]
    private string _statusLabel = "A carregar…";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private FlightDto? _selectedFlight;

    [ObservableProperty]
    private GateDto? _selectedGate;

    [ObservableProperty]
    private bool _isGateEditorVisible;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private bool _hasSuccess;


    // =========================================================
    // COLEÇÕES
    // =========================================================

    public ObservableCollection<FlightDto> GatesFlights { get; }
        = new();

    public ObservableCollection<GateDto> AvailableGates { get; }
        = new();


    // =========================================================
    // PROPRIEDADES AUXILIARES
    // =========================================================

    public string SelectedFlightTitle =>
        SelectedFlight == null
            ? string.Empty
            : $"{SelectedFlight.FlightNumber} · " +
              $"{SelectedFlight.Origin} → " +
              $"{SelectedFlight.Destination}";

    public string CurrentGate =>
        string.IsNullOrWhiteSpace(SelectedFlight?.Gate)
            ? "Sem porta"
            : SelectedFlight.Gate;


    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public OperationsGatesViewModel(
        ApiService apiService)
    {
        _apiService = apiService;
    }


    // =========================================================
    // ALTERAÇÕES DE PROPRIEDADES
    // =========================================================

    partial void OnSelectedFlightChanged(
        FlightDto? value)
    {
        OnPropertyChanged(
            nameof(SelectedFlightTitle));

        OnPropertyChanged(
            nameof(CurrentGate));
    }


    // =========================================================
    // LIMPAR MENSAGENS
    // =========================================================

    private void ClearMessages()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        HasSuccess = false;
        SuccessMessage = string.Empty;
    }


    // =========================================================
    // MOSTRAR ERRO
    // =========================================================

    private void ShowError(string message)
    {
        HasSuccess = false;
        SuccessMessage = string.Empty;

        ErrorMessage = message;
        HasError = true;
    }


    // =========================================================
    // MOSTRAR SUCESSO
    // =========================================================

    private void ShowSuccess(string message)
    {
        HasError = false;
        ErrorMessage = string.Empty;

        SuccessMessage = message;
        HasSuccess = true;
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

            HasError = false;
            ErrorMessage = string.Empty;

            StatusLabel = "A carregar…";

            var result =
                await _apiService.GetDeparturesAsync();

            if (!result.Success)
            {
                GatesFlights.Clear();

                StatusLabel =
                    result.ErrorMessage ??
                    "Não foi possível carregar as portas.";

                ShowError(
                    result.ErrorMessage ??
                    "Não foi possível carregar as portas.");

                return;
            }

            var flights =
                (result.Data ??
                 new List<FlightDto>())
                .Where(f =>
                    !string.Equals(
                        f.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.DepartureTime)
                .ToList();

            GatesFlights.Clear();

            foreach (var flight in flights)
            {
                GatesFlights.Add(flight);
            }

            StatusLabel = flights.Count switch
            {
                0 => "Nenhum voo disponível.",
                1 => "1 voo disponível.",
                _ => $"{flights.Count} voos disponíveis."
            };


            // =====================================================
            // ABRIR DIRETAMENTE O VOO RECEBIDO
            // =====================================================

            if (FlightId > 0)
            {
                var selectedFlight =
                    GatesFlights.FirstOrDefault(
                        f => f.Id == FlightId);

                FlightId = 0;

                if (selectedFlight != null)
                {
                    await LoadGateEditorAsync(
                        selectedFlight);
                }
                else
                {
                    ShowError(
                        "O voo selecionado não foi encontrado.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[OperationsGates] Load error: {ex}");

            GatesFlights.Clear();

            StatusLabel =
                "Não foi possível carregar as operações.";

            ShowError(
                "Ocorreu um erro ao carregar os voos.");
        }
        finally
        {
            IsBusy = false;
        }
    }


    // =========================================================
    // CARREGAR EDITOR DA PORTA
    // =========================================================

    private async Task LoadGateEditorAsync(
        FlightDto flight)
    {
        HasError = false;
        ErrorMessage = string.Empty;

        SelectedFlight = flight;
        SelectedGate = null;

        var result =
            await _apiService
                .GetEmployeeGatesAsync();

        if (!result.Success)
        {
            ShowError(
                result.ErrorMessage ??
                "Não foi possível carregar as portas.");

            return;
        }

        AvailableGates.Clear();

        var gates =
            (result.Data ??
             new List<GateDto>())
            .Where(g =>
                !string.Equals(
                    g.Status,
                    "Maintenance",
                    StringComparison.OrdinalIgnoreCase))
            .OrderBy(g => g.Terminal)
            .ThenBy(g => g.GateNumber)
            .ToList();

        foreach (var gate in gates)
        {
            AvailableGates.Add(gate);
        }


        // =====================================================
        // SELECIONAR A PORTA ATUAL
        // =====================================================

        SelectedGate =
            AvailableGates.FirstOrDefault(
                g =>
                    string.Equals(
                        g.GateNumber,
                        flight.Gate,
                        StringComparison.OrdinalIgnoreCase));

        IsGateEditorVisible = true;
    }


    // =========================================================
    // ABRIR EDITOR MANUALMENTE
    // =========================================================

    [RelayCommand]
    private async Task OpenGateEditorAsync(
        FlightDto? flight)
    {
        if (flight == null || IsBusy)
            return;

        try
        {
            IsBusy = true;

            ClearMessages();

            await LoadGateEditorAsync(flight);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[OperationsGates] Load gates error: {ex}");

            ShowError(
                "Não foi possível carregar as portas disponíveis.");
        }
        finally
        {
            IsBusy = false;
        }
    }


    // =========================================================
    // CANCELAR ALTERAÇÃO
    // =========================================================

    [RelayCommand]
    private void CancelGateChange()
    {
        IsGateEditorVisible = false;

        SelectedFlight = null;
        SelectedGate = null;

        HasError = false;
        ErrorMessage = string.Empty;
    }


    // =========================================================
    // CONFIRMAR ALTERAÇÃO DA PORTA
    // =========================================================

    [RelayCommand]
    private async Task ConfirmGateChangeAsync()
    {
        if (IsBusy)
            return;


        // =====================================================
        // VALIDAR VOO
        // =====================================================

        if (SelectedFlight == null)
        {
            ShowError(
                "Nenhum voo foi selecionado.");

            return;
        }


        // =====================================================
        // VALIDAR PORTA
        // =====================================================

        if (SelectedGate == null)
        {
            ShowError(
                "Selecione a nova porta.");

            return;
        }


        // =====================================================
        // NÃO PERMITIR A MESMA PORTA
        // =====================================================

        if (string.Equals(
            SelectedFlight.Gate,
            SelectedGate.GateNumber,
            StringComparison.OrdinalIgnoreCase))
        {
            ShowError(
                "Esta já é a porta atribuída ao voo.");

            return;
        }


        // Guardamos os dados antes da alteração.
        var flightNumber =
            SelectedFlight.FlightNumber;

        var oldGate =
            CurrentGate;

        var newGate =
            SelectedGate.GateNumber;


        // =====================================================
        // CONFIRMAÇÃO
        // =====================================================

        var confirmed =
            await Shell.Current.DisplayAlert(
                "Alterar porta",
                $"Alterar o voo {flightNumber} " +
                $"da porta {oldGate} " +
                $"para {newGate}?",
                "Alterar",
                "Cancelar");

        if (!confirmed)
            return;


        // =====================================================
        // ALTERAR PORTA
        // =====================================================

        try
        {
            IsBusy = true;

            HasError = false;
            ErrorMessage = string.Empty;

            var result =
                await _apiService
                    .ChangeFlightGateAsync(
                        SelectedFlight.Id,
                        SelectedGate.Id);

            if (!result.Success ||
                result.Data == null)
            {
                ShowError(
                    result.ErrorMessage ??
                    "Não foi possível alterar a porta.");

                return;
            }


            // =================================================
            // FECHAR EDITOR
            // =================================================

            IsGateEditorVisible = false;

            SelectedFlight = null;
            SelectedGate = null;


            // =================================================
            // ATUALIZAR LISTA
            // =================================================

            await ReloadAfterChangeAsync();


            // =================================================
            // FEEDBACK VISUAL
            // =================================================

            ShowSuccess(
                $"Porta atualizada: voo {flightNumber} " +
                $"alterado de {oldGate} para {newGate}.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[OperationsGates] Change gate error: {ex}");

            ShowError(
                "Ocorreu um erro ao alterar a porta.");
        }
        finally
        {
            IsBusy = false;
        }
    }


    // =========================================================
    // ATUALIZAR LISTA DEPOIS DA ALTERAÇÃO
    // =========================================================

    private async Task ReloadAfterChangeAsync()
    {
        var result =
            await _apiService.GetDeparturesAsync();

        if (!result.Success)
        {
            StatusLabel =
                "Porta alterada, mas não foi possível " +
                "atualizar a lista.";

            return;
        }

        var flights =
            (result.Data ??
             new List<FlightDto>())
            .Where(f =>
                !string.Equals(
                    f.Status,
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f.DepartureTime)
            .ToList();

        GatesFlights.Clear();

        foreach (var flight in flights)
        {
            GatesFlights.Add(flight);
        }

        StatusLabel = flights.Count switch
        {
            0 => "Nenhum voo disponível.",
            1 => "1 voo disponível.",
            _ => $"{flights.Count} voos disponíveis."
        };
    }
}