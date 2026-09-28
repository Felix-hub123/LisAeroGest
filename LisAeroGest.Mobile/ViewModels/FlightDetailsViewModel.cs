using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using LisAeroGest.Mobile.Views;
using System.Diagnostics;

namespace LisAeroGest.Mobile.ViewModels
{
    public partial class FlightDetailsViewModel :
        ObservableObject,
        IQueryAttributable
    {
        private readonly ApiService _apiService;
        private readonly FavoritesService _favoritesService;


        // =========================================================
        // VOO
        // =========================================================

        [ObservableProperty]
        private int _flightId;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasFlight))]
        private FlightDetailDto? _flight;


        // =========================================================
        // ESTADO
        // =========================================================

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;


        // =========================================================
        // FAVORITOS
        // =========================================================

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FavoriteButtonText))]
        private bool _isFavorite;

        [ObservableProperty]
        private string _successMessage = string.Empty;

        [ObservableProperty]
        private bool _hasSuccess;


        // =========================================================
        // PROPRIEDADES AUXILIARES
        // =========================================================

        public bool HasFlight =>
            Flight != null;

        public string FavoriteButtonText =>
            IsFavorite
                ? "★ Favorito"
                : "☆ Favorito";


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public FlightDetailsViewModel(
            ApiService apiService,
            FavoritesService favoritesService)
        {
            _apiService = apiService;
            _favoritesService = favoritesService;
        }


        // =========================================================
        // NAVEGAÇÃO
        // =========================================================

        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            if (!query.TryGetValue(
                    "flightId",
                    out var raw)
                ||
                raw == null)
            {
                return;
            }


            if (raw is int value)
            {
                FlightId = value;
            }
            else if (int.TryParse(
                         raw.ToString(),
                         out var parsed))
            {
                FlightId = parsed;
            }
        }


        // =========================================================
        // FLIGHT ID ALTERADO
        // =========================================================

        partial void OnFlightIdChanged(
            int value)
        {
            if (value <= 0)
                return;


            MainThread.BeginInvokeOnMainThread(
                async () =>
                {
                    await LoadAsync();
                });
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
        // CARREGAR VOO
        // =========================================================

        [RelayCommand]
        public async Task LoadAsync()
        {
            if (FlightId <= 0 ||
                IsBusy)
            {
                return;
            }


            try
            {
                IsBusy = true;

                ClearFeedback();


                var result =
                    await _apiService
                        .GetDetailsAsync(
                            FlightId);


                // =================================================
                // ERRO
                // =================================================

                if (!result.Success ||
                    result.Data == null)
                {
                    Flight = null;

                    ShowError(
                        result.ErrorMessage ??
                        "Não foi possível encontrar os detalhes do voo.");

                    return;
                }


                // =================================================
                // VOO
                // =================================================

                Flight =
                    result.Data;


                // =================================================
                // VERIFICAR FAVORITO
                // =================================================

                IsFavorite =
                    _favoritesService
                        .IsFavorite(
                            Flight.Id);
            }
            catch (Exception ex)
            {
                Flight = null;

                ShowError(
                    "Ocorreu um erro ao carregar os detalhes do voo.");


                Debug.WriteLine(
                    $"[FlightDetailsViewModel] {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // ADICIONAR / REMOVER FAVORITO
        // =========================================================

        [RelayCommand]
        private void ToggleFavorite()
        {
            if (Flight == null ||
                IsBusy)
            {
                return;
            }


            try
            {
                ClearFeedback();


                IsFavorite =
                    _favoritesService
                        .Toggle(
                            Flight.Id);


                if (IsFavorite)
                {
                    ShowSuccess(
                        $"O voo {Flight.FlightNumber} foi adicionado aos favoritos.");
                }
                else
                {
                    ShowSuccess(
                        $"O voo {Flight.FlightNumber} foi removido dos favoritos.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"[FlightDetailsViewModel] Erro nos favoritos: {ex}");


                ShowError(
                    "Não foi possível atualizar os favoritos.");
            }
        }


        // =========================================================
        // ESCOLHER LUGAR
        // =========================================================

        [RelayCommand]
        private async Task SelectSeatAsync()
        {
            if (FlightId <= 0 ||
                IsBusy)
            {
                return;
            }


            await Shell.Current.GoToAsync(
                $"{nameof(SelectSeatPage)}?flightId={FlightId}");
        }
    }
}