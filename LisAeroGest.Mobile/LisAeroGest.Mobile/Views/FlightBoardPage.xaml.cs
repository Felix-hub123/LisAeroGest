using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
   
    public partial class FlightBoardPage : ContentPage
    {
        private readonly FlightBoardViewModel _viewModel;

        public FlightBoardPage(FlightBoardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Carrega os dados da API automaticamente ao abrir a página
            if (_viewModel.LoadDeparturesCommand.CanExecute(null))
            {
                await _viewModel.LoadDeparturesCommand.ExecuteAsync(null);
            }
        }
    }
}