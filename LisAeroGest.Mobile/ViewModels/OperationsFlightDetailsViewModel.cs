using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;
using LisAeroGest.Mobile.Views;


namespace LisAeroGest.Mobile.ViewModels;

[QueryProperty(nameof(FlightId), "flightId")]
public partial class OperationsFlightDetailsViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private int _flightId;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private EmployeeFlightOperationDto? _operation;

    public ObservableCollection<EmployeeFlightPassengerDto>
        Passengers
    { get; } = new();

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasOperation =>
        Operation != null;

    public OperationsFlightDetailsViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnOperationChanged(EmployeeFlightOperationDto? value)
    {
        OnPropertyChanged(nameof(HasOperation));
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy || FlightId <= 0)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var result =
                await _apiService.GetEmployeeFlightOperationAsync(
                    FlightId);

            if (!result.Success || result.Data == null)
            {
                ErrorMessage =
                    result.ErrorMessage ??
                    "Não foi possível carregar a operação.";

                return;
            }

            Operation = result.Data;

            Passengers.Clear();

            foreach (var passenger in result.Data.Passengers)
            {
                Passengers.Add(passenger);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenPassengersAsync()
    {
        if (Operation == null)
            return;

        await Shell.Current.GoToAsync(
            nameof(OperationsPassengersPage));
    }

    [RelayCommand]
    private async Task OpenGatesAsync()
    {
        await Shell.Current.GoToAsync(
            nameof(OperationsGatesPage));
    }
}