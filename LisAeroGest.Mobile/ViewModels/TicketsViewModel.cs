using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class TicketsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly AuthService _authService;

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<TicketDto> Tickets { get; } = new();

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        public TicketsViewModel(ApiService apiService, AuthService authService)
        {
            _apiService = apiService;
            _authService = authService;
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
          
            if (Shell.Current is AppShell shell)
            {
                await shell.LogoutAsync();
            }
        }

        [RelayCommand]
        public async Task LoadTicketsAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var result = await _apiService.GetMyTicketsAsync();
                Tickets.Clear();

                if (result == null || !result.Success)
                {
                    ErrorMessage = result?.ErrorMessage
                        ?? "Não foi possível carregar os bilhetes.";
                    HasError = true;
                    return;
                }

                foreach (var ticket in result.Data ?? new List<TicketDto>())
                    Tickets.Add(ticket);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Erro ao carregar bilhetes.";
                HasError = true;
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task OpenTicketAsync(TicketDto? ticket)
        {
            if (ticket == null)
                return;

            if (ticket.Status is "Paid" or "CheckedIn")
            {
            
                await Shell.Current.GoToAsync(
                    nameof(CheckInPage),
                    new Dictionary<string, object>
                    {
                        ["ticketId"] = ticket.Id
                    });

                return;
            }

            await Shell.Current.DisplayAlert(
                "Bilhete",
                $"Estado atual: {ticket.Status}.",
                "OK");
        }
    }
}