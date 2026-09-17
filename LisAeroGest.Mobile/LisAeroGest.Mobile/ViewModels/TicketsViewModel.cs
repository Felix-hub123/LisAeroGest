using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Helpers;
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
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<TicketDto> Tickets { get; } = new();
        [ObservableProperty] private string _errorMessage = string.Empty;
        [ObservableProperty] private bool _hasError;
        public TicketsViewModel(ApiService apiService, AuthService authService, IServiceProvider serviceProvider)
        {
            _apiService = apiService;
            _authService = authService;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            _authService.Logout();
            Application.Current!.MainPage = _serviceProvider.GetRequiredService<LoginPage>();
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
                    ErrorMessage = result?.ErrorMessage ?? "Não foi possível carregar os bilhetes.";
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
                PendingBooking.TicketId = ticket.Id;

                await Shell.Current.GoToAsync(
                    nameof(CheckInPage));

                return;
            }

            await Shell.Current.DisplayAlert(
                "Bilhete",
                $"Estado atual: {ticket.Status}.",
                "OK");
        }
    }
}
