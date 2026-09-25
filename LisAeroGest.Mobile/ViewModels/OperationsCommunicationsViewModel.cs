using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;

namespace LisAeroGest.Mobile.ViewModels;

public partial class OperationsCommunicationsViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private int _flightId;

    [ObservableProperty]
    private FlightDto? _selectedFlight;

    [ObservableProperty]
    private string _selectedType = "Informação";

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _statusLabel = string.Empty;

    public ObservableCollection<FlightDto> Flights { get; } = new();

    public List<string> CommunicationTypes { get; } = new()
    {
        "Informação",
        "Embarque",
        "Porta",
        "Atraso"
    };

    public bool HasSelectedFlight =>
        SelectedFlight != null;

    public string SelectedFlightTitle =>
        SelectedFlight == null
            ? "Selecione um voo"
            : $"{SelectedFlight.FlightNumber} · " +
              $"{SelectedFlight.Origin} → " +
              $"{SelectedFlight.Destination}";

    public OperationsCommunicationsViewModel(
        ApiService apiService)
    {
        _apiService = apiService;
    }

    partial void OnSelectedFlightChanged(FlightDto? value)
    {
        OnPropertyChanged(nameof(HasSelectedFlight));
        OnPropertyChanged(nameof(SelectedFlightTitle));
    }

    // CARREGAR VOOS
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
            StatusLabel = string.Empty;

            var result =
                await _apiService.GetDeparturesAsync();

            if (!result.Success)
            {
                Flights.Clear();

                ErrorMessage =
                    result.ErrorMessage ??
                    "Não foi possível carregar os voos.";

                HasError = true;
                return;
            }

            var flights =
                (result.Data ?? new List<FlightDto>())
                .Where(f =>
                    !string.Equals(
                        f.Status,
                        "Cancelled",
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.DepartureTime)
                .ToList();

            Flights.Clear();

            foreach (var flight in flights)
            {
                Flights.Add(flight);
            }

            // Se chegámos através dos detalhes de um voo,
            // selecionamos automaticamente esse voo.
            if (FlightId > 0)
            {
                SelectedFlight =
                    Flights.FirstOrDefault(
                        f => f.Id == FlightId);
            }

            // Se existir apenas um voo, seleciona-o.
            if (SelectedFlight == null &&
                Flights.Count == 1)
            {
                SelectedFlight = Flights[0];
            }

            if (Flights.Count == 0)
            {
                StatusLabel =
                    "Não existem voos disponíveis.";
            }
        }
        catch (Exception)
        {
            ErrorMessage =
                "Ocorreu um erro ao carregar os voos.";

            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ENVIAR COMUNICAÇÃO
    [RelayCommand]
    private async Task SendAsync()
    {
        if (IsBusy)
            return;

        HasError = false;
        ErrorMessage = string.Empty;
        StatusLabel = string.Empty;

        if (SelectedFlight == null)
        {
            ErrorMessage =
                "Selecione primeiro um voo.";

            HasError = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(Message))
        {
            ErrorMessage =
                "Escreva a mensagem.";

            HasError = true;
            return;
        }

        if (Message.Trim().Length > 500)
        {
            ErrorMessage =
                "A mensagem não pode exceder 500 caracteres.";

            HasError = true;
            return;
        }

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

            var request =
                new FlightCommunicationRequestDto
                {
                    Type = ConvertType(SelectedType),
                    Message = Message.Trim()
                };

            var result =
                await _apiService
                    .SendFlightCommunicationAsync(
                        SelectedFlight.Id,
                        request);

            if (!result.Success ||
                result.Data == null)
            {
                ErrorMessage =
                    result.ErrorMessage ??
                    "Não foi possível enviar a comunicação.";

                HasError = true;
                return;
            }

            StatusLabel =
                $"✓ Enviado a " +
                $"{result.Data.Recipients} passageiro(s).";

            Message = string.Empty;

            await Shell.Current.DisplayAlert(
                "Comunicação enviada",
                result.Data.Message,
                "OK");
        }
        catch (Exception)
        {
            ErrorMessage =
                "Ocorreu um erro ao enviar a comunicação.";

            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string ConvertType(string type)
    {
        return type switch
        {
            "Embarque" => "Boarding",
            "Porta" => "Gate",
            "Atraso" => "Delay",
            _ => "Info"
        };
    }
}