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

    // ID recebido através da página de operação.
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

    public ObservableCollection<FlightDto> GatesFlights { get; }
        = new();

    public ObservableCollection<GateDto> AvailableGates { get; }
        = new();

    public string SelectedFlightTitle =>
        SelectedFlight == null
            ? string.Empty
            : $"{SelectedFlight.FlightNumber} · " +
              $"{SelectedFlight.Origin} → " +
              $"{SelectedFlight.Destination}";

    public string CurrentGate =>
        string.IsNullOrWhiteSpace(
            SelectedFlight?.Gate)
            ? "Sem porta"
            : SelectedFlight.Gate;

    public OperationsGatesViewModel(
        ApiService apiService)
    {
        _apiService = apiService;
    }

    partial void OnSelectedFlightChanged(
        FlightDto? value)
    {
        OnPropertyChanged(
            nameof(SelectedFlightTitle));

        OnPropertyChanged(
            nameof(CurrentGate));
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

            // -------------------------------------------------
            // Se veio da operação de um voo,
            // abre diretamente esse voo.
            // -------------------------------------------------

            if (FlightId > 0)
            {
                var selectedFlight =
                    GatesFlights.FirstOrDefault(
                        f => f.Id == FlightId);

                // Limpar para não voltar a abrir
                // automaticamente num refresh.
                FlightId = 0;

                if (selectedFlight != null)
                {
                    await LoadGateEditorAsync(
                        selectedFlight);
                }
                else
                {
                    ErrorMessage =
                        "O voo selecionado não foi encontrado.";

                    HasError = true;
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

            ErrorMessage =
                "Ocorreu um erro ao carregar os voos.";

            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // =========================================================
    // CARREGAR EDITOR
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
            ErrorMessage =
                result.ErrorMessage ??
                "Não foi possível carregar as portas.";

            HasError = true;

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

        // Selecionar automaticamente
        // a porta atual do voo.
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

            await LoadGateEditorAsync(flight);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[OperationsGates] Load gates error: {ex}");

            ErrorMessage =
                "Não foi possível carregar as portas disponíveis.";

            HasError = true;
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
    // CONFIRMAR ALTERAÇÃO
    // =========================================================

    [RelayCommand]
    private async Task ConfirmGateChangeAsync()
    {
        if (IsBusy)
            return;

        if (SelectedFlight == null)
        {
            ErrorMessage =
                "Nenhum voo foi selecionado.";

            HasError = true;

            return;
        }

        if (SelectedGate == null)
        {
            ErrorMessage =
                "Selecione a nova porta.";

            HasError = true;

            return;
        }

        // Não permite escolher a mesma porta.
        if (string.Equals(
            SelectedFlight.Gate,
            SelectedGate.GateNumber,
            StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage =
                "Esta já é a porta atribuída ao voo.";

            HasError = true;

            return;
        }

        var confirmed =
            await Shell.Current.DisplayAlert(
                "Alterar porta",
                $"Alterar o voo " +
                $"{SelectedFlight.FlightNumber} " +
                $"da porta {CurrentGate} " +
                $"para {SelectedGate.GateNumber}?",
                "Alterar",
                "Cancelar");

        if (!confirmed)
            return;

        try
        {
            IsBusy = true;

            HasError = false;
            ErrorMessage = string.Empty;

            var flightNumber =
                SelectedFlight.FlightNumber;

            var newGate =
                SelectedGate.GateNumber;

            var result =
                await _apiService
                    .ChangeFlightGateAsync(
                        SelectedFlight.Id,
                        SelectedGate.Id);

            if (!result.Success ||
                result.Data == null)
            {
                ErrorMessage =
                    result.ErrorMessage ??
                    "Não foi possível alterar a porta.";

                HasError = true;

                return;
            }

            IsGateEditorVisible = false;

            await Shell.Current.DisplayAlert(
                "Porta atualizada",
                $"O voo {flightNumber} " +
                $"foi atribuído à porta {newGate}.",
                "OK");

            SelectedFlight = null;
            SelectedGate = null;

            await ReloadAfterChangeAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"[OperationsGates] Change gate error: {ex}");

            ErrorMessage =
                "Ocorreu um erro ao alterar a porta.";

            HasError = true;
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