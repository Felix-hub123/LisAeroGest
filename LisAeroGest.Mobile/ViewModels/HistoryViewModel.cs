using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;

namespace LisAeroGest.Mobile.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string errorMessage = string.Empty;

    public ObservableCollection<TicketDto> Tickets { get; } = new();

    public HistoryViewModel(ApiService apiService) => _apiService = apiService;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            var result = await _apiService.GetMyTicketsAsync();
            Tickets.Clear();
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage ?? "Não foi possível carregar o histórico.";
                return;
            }

            foreach (var ticket in (result.Data ?? new List<TicketDto>())
                .Where(t => t.DepartureTime < DateTime.Now)
                .OrderByDescending(t => t.DepartureTime))
                Tickets.Add(ticket);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
