using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;
using System.Collections.ObjectModel;

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
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var tickets = await _apiService.GetMyTicketsAsync();

                Tickets.Clear();
                foreach (var ticket in tickets)
                {
                    Tickets.Add(ticket);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar bilhetes: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
