using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;
using System.Collections.ObjectModel;

namespace LisAeroGest.Mobile.ViewModels;

[QueryProperty(nameof(FlightId), "flightId")]
public partial class OperationsFlightDetailsViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    private List<EmployeeFlightPassengerDto> _allPassengers = new();

    [ObservableProperty]
    private int _flightId;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private EmployeeFlightOperationDto? _operation;


    // =========================================================
    // METEOROLOGIA
    // =========================================================

    [ObservableProperty]
    private WeatherDto? _originWeather;

    [ObservableProperty]
    private WeatherDto? _destinationWeather;

    [ObservableProperty]
    private string _weatherAlertMessage = string.Empty;


    // =========================================================
    // PASSAGEIROS
    // =========================================================

    public ObservableCollection<EmployeeFlightPassengerDto> Passengers { get; }
        = new();


    // =========================================================
    // PROPRIEDADES AUXILIARES
    // =========================================================

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasSuccess =>
    !string.IsNullOrWhiteSpace(SuccessMessage);

    public bool HasOperation =>
        Operation != null;

    public bool HasPassengers =>
        Passengers.Count > 0;

    public bool HasNoPassengers =>
        Passengers.Count == 0;

    public bool HasOriginWeather =>
        OriginWeather != null;

    public bool HasDestinationWeather =>
        DestinationWeather != null;

    public bool HasWeather =>
        OriginWeather != null ||
        DestinationWeather != null;

    public bool HasWeatherAlert =>
        !string.IsNullOrWhiteSpace(WeatherAlertMessage);


    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public OperationsFlightDetailsViewModel(
        ApiService apiService)
    {
        _apiService = apiService;

        Passengers.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasPassengers));
            OnPropertyChanged(nameof(HasNoPassengers));
        };
    }


    // =========================================================
    // ALTERAÇÕES DE PROPRIEDADES
    // =========================================================

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnSuccessMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasSuccess));
    }

    partial void OnOperationChanged(
        EmployeeFlightOperationDto? value)
    {
        OnPropertyChanged(nameof(HasOperation));
    }

    partial void OnOriginWeatherChanged(WeatherDto? value)
    {
        OnPropertyChanged(nameof(HasOriginWeather));
        OnPropertyChanged(nameof(HasWeather));
    }

    partial void OnDestinationWeatherChanged(WeatherDto? value)
    {
        OnPropertyChanged(nameof(HasDestinationWeather));
        OnPropertyChanged(nameof(HasWeather));
    }

    partial void OnWeatherAlertMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasWeatherAlert));
    }


    // =========================================================
    // CARREGAR OPERAÇÃO
    // =========================================================

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy || FlightId <= 0)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            OriginWeather = null;
            DestinationWeather = null;
            WeatherAlertMessage = string.Empty;

            var result =
                await _apiService
                    .GetEmployeeFlightOperationAsync(
                        FlightId);

            if (!result.Success ||
                result.Data == null)
            {
                ErrorMessage =
                    result.ErrorMessage ??
                    "Não foi possível carregar a operação.";

                return;
            }

            Operation = result.Data;

            Passengers.Clear();

            _allPassengers =
                result.Data.Passengers?.ToList()
                ?? new List<EmployeeFlightPassengerDto>();

            ApplyPassengerFilter();

            // Carregar meteorologia depois de carregar o voo.
            await LoadWeatherAsync();
        }
        catch (Exception)
        {
            ErrorMessage =
                "Ocorreu um erro ao carregar a operação.";
        }
        finally
        {
            IsBusy = false;
        }
    }


    // =========================================================
    // CARREGAR METEOROLOGIA
    // =========================================================

    private async Task LoadWeatherAsync()
    {
        if (Operation == null)
            return;

        OriginWeather = null;
        DestinationWeather = null;
        WeatherAlertMessage = string.Empty;

        // -------------------------
        // ORIGEM
        // -------------------------

        if (!string.IsNullOrWhiteSpace(Operation.Origin))
        {
            var originResult =
                await _apiService.GetWeatherAsync(
                    Operation.Origin);

            if (originResult.Success &&
                originResult.Data != null)
            {
                OriginWeather = originResult.Data;
            }
        }

        // -------------------------
        // DESTINO
        // -------------------------

        if (!string.IsNullOrWhiteSpace(Operation.Destination))
        {
            var destinationResult =
                await _apiService.GetWeatherAsync(
                    Operation.Destination);

            if (destinationResult.Success &&
                destinationResult.Data != null)
            {
                DestinationWeather =
                    destinationResult.Data;
            }
        }

        // -------------------------
        // ALERTAS METEOROLÓGICOS
        // -------------------------

        var alerts = new List<string>();

        CheckWeatherAlert(
            OriginWeather,
            "Origem",
            alerts);

        CheckWeatherAlert(
            DestinationWeather,
            "Destino",
            alerts);

        WeatherAlertMessage =
            string.Join(" • ", alerts);
    }


    // =========================================================
    // VERIFICAR CONDIÇÕES METEOROLÓGICAS
    // =========================================================

    private static void CheckWeatherAlert(
        WeatherDto? weather,
        string location,
        List<string> alerts)
    {
        if (weather == null)
            return;

        if (weather.WindSpeed >= 40)
        {
            alerts.Add(
                $"{location}: vento forte");
        }

        if (weather.Visibility < 5)
        {
            alerts.Add(
                $"{location}: visibilidade reduzida");
        }

        var condition =
            weather.Description?.ToLowerInvariant()
            ?? string.Empty;

        if (condition.Contains("trovoada") ||
            condition.Contains("thunderstorm"))
        {
            alerts.Add(
                $"{location}: trovoada");
        }
    }


    // =========================================================
    // PESQUISA DE PASSAGEIROS
    // =========================================================

    partial void OnSearchTextChanged(string value)
    {
        ApplyPassengerFilter();
    }

    private void ApplyPassengerFilter()
    {
        IEnumerable<EmployeeFlightPassengerDto> query =
            _allPassengers;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim();

            query = query.Where(p =>
                p.PassengerName.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase)
                ||
                p.DocumentNumber.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase)
                ||
                p.TicketId.ToString().Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase));
        }

        Passengers.Clear();

        foreach (var passenger in query)
        {
            Passengers.Add(passenger);
        }
    }


    // =========================================================
    // CHECK-IN DO PASSAGEIRO
    // =========================================================

    [RelayCommand]
    private async Task CheckInPassengerAsync(
      EmployeeFlightPassengerDto? passenger)
    {
        if (passenger == null || IsBusy)
            return;

        SuccessMessage = string.Empty;
        ErrorMessage = string.Empty;

        if (!passenger.CanCheckIn)
        {
            ErrorMessage =
                "Este passageiro não está disponível para check-in.";

            return;
        }

        var confirmed =
            await Shell.Current.DisplayAlert(
                "Confirmar check-in",
                $"Fazer check-in de {passenger.PassengerName}?",
                "Confirmar",
                "Cancelar");

        if (!confirmed)
            return;

        try
        {
            IsBusy = true;

            var result =
                await _apiService.EmployeeCheckInAsync(
                    passenger.TicketId);

            if (!result.Success)
            {
                ErrorMessage =
                    result.ErrorMessage ??
                    "Não foi possível realizar o check-in.";

                return;
            }

            SuccessMessage =
                $"Check-in de {passenger.PassengerName} realizado com sucesso.";
        }
        catch
        {
            ErrorMessage =
                "Ocorreu um erro ao realizar o check-in.";
        }
        finally
        {
            IsBusy = false;
        }

        if (!string.IsNullOrWhiteSpace(SuccessMessage))
        {
            await LoadAsync();
        }
    }


    // =========================================================
    // GERIR PORTA DESTE VOO
    // =========================================================

    [RelayCommand]
    private async Task OpenGatesAsync()
    {
        if (Operation == null)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(OperationsGatesPage)}" +
            $"?flightId={Operation.FlightId}");
    }


    // =========================================================
    // COMUNICAÇÕES DESTE VOO
    // =========================================================

    [RelayCommand]
    private async Task OpenCommunicationsAsync()
    {
        if (Operation == null)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(OperationsCommunicationsPage)}" +
            $"?flightId={Operation.FlightId}");
    }
}