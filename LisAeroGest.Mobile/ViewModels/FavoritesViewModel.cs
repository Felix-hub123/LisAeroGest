using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;

namespace LisAeroGest.Mobile.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly FavoritesService _favoritesService;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string errorMessage = string.Empty;

    public ObservableCollection<FlightDetailDto> Flights { get; } = new();

    public FavoritesViewModel(ApiService apiService, FavoritesService favoritesService)
    {
        _apiService = apiService;
        _favoritesService = favoritesService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            Flights.Clear();
            foreach (var id in _favoritesService.GetFavoriteFlightIds())
            {
                var result = await _apiService.GetDetailsAsync(id);
                if (result.Success && result.Data != null)
                    Flights.Add(result.Data);
                else
                    _favoritesService.Remove(id);
            }
        }
        catch
        {
            ErrorMessage = "Não foi possível carregar os favoritos.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenFlightAsync(FlightDetailDto? flight)
    {
        if (flight == null) return;
        await Shell.Current.GoToAsync(nameof(Views.FlightDetailsPage), new Dictionary<string, object>
        {
            ["flightId"] = flight.Id
        });
    }

    [RelayCommand]
    private async Task RemoveAsync(FlightDetailDto? flight)
    {
        if (flight == null) return;
        _favoritesService.Remove(flight.Id);
        await LoadAsync();
    }
}
