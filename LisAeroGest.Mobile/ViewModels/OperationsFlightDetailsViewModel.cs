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

    [ObservableProperty]
    private int _flightId;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private EmployeeFlightOperationDto? _operation;

    public ObservableCollection<EmployeeFlightPassengerDto> Passengers { get; }
        = new();

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasOperation =>
        Operation != null;

    public bool HasPassengers =>
        Passengers.Count > 0;

    public bool HasNoPassengers =>
        Passengers.Count == 0;

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

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnOperationChanged(
        EmployeeFlightOperationDto? value)
    {
        OnPropertyChanged(nameof(HasOperation));
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

            foreach (var passenger
                     in result.Data.Passengers)
            {
                Passengers.Add(passenger);
            }
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
    // CHECK-IN DO PASSAGEIRO
    // =========================================================

    [RelayCommand]
    private async Task CheckInPassengerAsync(
        EmployeeFlightPassengerDto? passenger)
    {
        if (passenger == null)
            return;

        if (!passenger.CanCheckIn)
        {
            await Shell.Current.DisplayAlert(
                "Check-in",
                "Este passageiro não está disponível para check-in.",
                "OK");

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
                await Shell.Current.DisplayAlert(
                    "Erro",
                    result.ErrorMessage ??
                    "Não foi possível realizar o check-in.",
                    "OK");

                return;
            }

            await Shell.Current.DisplayAlert(
                "Check-in concluído",
                $"Check-in de {passenger.PassengerName} " +
                "realizado com sucesso.",
                "OK");
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert(
                "Erro",
                "Ocorreu um erro ao realizar o check-in.",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }

        // Atualizar dados depois do check-in.
        await LoadAsync();
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
}