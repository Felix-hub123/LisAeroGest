using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class FavoritesViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly FavoritesService _favoritesService;


        // =========================================================
        // ESTADO
        // =========================================================

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private string _successMessage = string.Empty;

        [ObservableProperty]
        private bool _hasSuccess;


        // =========================================================
        // FAVORITOS
        // =========================================================

        public ObservableCollection<FlightDetailDto> Flights
        {
            get;
        } = new();


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public FavoritesViewModel(
            ApiService apiService,
            FavoritesService favoritesService)
        {
            _apiService = apiService;
            _favoritesService = favoritesService;
        }


        // =========================================================
        // FEEDBACK
        // =========================================================

        private void ClearFeedback()
        {
            HasError = false;
            ErrorMessage = string.Empty;

            HasSuccess = false;
            SuccessMessage = string.Empty;
        }


        private void ShowError(
            string message)
        {
            HasSuccess = false;
            SuccessMessage = string.Empty;

            ErrorMessage = message;
            HasError = true;
        }


        private void ShowSuccess(
            string message)
        {
            HasError = false;
            ErrorMessage = string.Empty;

            SuccessMessage = message;
            HasSuccess = true;
        }


        // =========================================================
        // CARREGAR FAVORITOS
        // =========================================================

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                ClearFeedback();

                Flights.Clear();


                var favoriteIds =
                    _favoritesService
                        .GetFavoriteFlightIds();


                // =================================================
                // SEM FAVORITOS
                // =================================================

                if (favoriteIds.Count == 0)
                    return;


                // =================================================
                // CARREGAR CADA VOO
                // =================================================

                var failedCount = 0;


                foreach (var id in favoriteIds)
                {
                    try
                    {
                        var result =
                            await _apiService
                                .GetDetailsAsync(id);


                        if (result.Success &&
                            result.Data != null)
                        {
                            Flights.Add(
                                result.Data);
                        }
                        else
                        {
                            // IMPORTANTE:
                            // não removemos automaticamente o favorito.
                            // A API pode simplesmente estar indisponível.
                            failedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;

                        Debug.WriteLine(
                            $"[Favoritos] Erro ao carregar voo {id}: {ex.Message}");
                    }
                }


                // =================================================
                // ALGUNS VOOS NÃO FORAM CARREGADOS
                // =================================================

                if (failedCount > 0)
                {
                    ShowError(
                        failedCount == 1
                            ? "Não foi possível carregar um dos voos favoritos."
                            : $"Não foi possível carregar {failedCount} voos favoritos.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[Favoritos] Erro: {ex}");

                ShowError(
                    "Não foi possível carregar os favoritos.");
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // ABRIR VOO
        // =========================================================

        [RelayCommand]
        private async Task OpenFlightAsync(
            FlightDetailDto? flight)
        {
            if (flight == null ||
                IsBusy)
            {
                return;
            }


            await Shell.Current.GoToAsync(
                nameof(Views.FlightDetailsPage),
                new Dictionary<string, object>
                {
                    ["flightId"] = flight.Id
                });
        }


        // =========================================================
        // REMOVER FAVORITO
        // =========================================================

        [RelayCommand]
        private async Task RemoveAsync(
            FlightDetailDto? flight)
        {
            if (flight == null ||
                IsBusy)
            {
                return;
            }


            // =====================================================
            // CONFIRMAÇÃO
            // =====================================================

            var confirmed =
                await Shell.Current.DisplayAlert(
                    "Remover favorito",
                    $"Pretendes remover o voo {flight.FlightNumber} dos favoritos?",
                    "Remover",
                    "Cancelar");


            if (!confirmed)
                return;


            try
            {
                IsBusy = true;

                ClearFeedback();


                // =================================================
                // REMOVER LOCALMENTE
                // =================================================

                _favoritesService.Remove(
                    flight.Id);


                Flights.Remove(
                    flight);


                // =================================================
                // FEEDBACK
                // =================================================

                ShowSuccess(
                    $"O voo {flight.FlightNumber} foi removido dos favoritos.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[Favoritos] Erro ao remover: {ex}");

                ShowError(
                    "Não foi possível remover o voo dos favoritos.");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}