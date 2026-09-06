using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class FlightBoardViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<FlightDto> Departures { get; } = new();

        // Construtor a receber o ApiService por injeção de dependências (boa prática .NET MAUI)
        public FlightBoardViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        
      

        [RelayCommand]
        public async Task LoadDeparturesAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var flights = await _apiService.GetDeparturesAsync();

                Departures.Clear();
                if (flights != null)
                {
                    foreach (var flight in flights)
                    {
                        Departures.Add(flight);
                    }
                }
            }
            catch (Exception ex)
            {
                // Evita que a app crashe em caso de falha na rede/API
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar partida: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}