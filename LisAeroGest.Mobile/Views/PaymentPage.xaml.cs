using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class PaymentPage :
        ContentPage,
        IQueryAttributable
    {
        private readonly PaymentViewModel _viewModel;


        // =========================================================
        // CONSTRUTOR
        // =========================================================

        public PaymentPage(
            PaymentViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            BindingContext = _viewModel;
        }


        // =========================================================
        // RECEBER PARÂMETROS
        // =========================================================

        public void ApplyQueryAttributes(
            IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(
                query);
        }
    }
}