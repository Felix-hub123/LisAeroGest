using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class PaymentPage : ContentPage, IQueryAttributable
    {
        public PaymentPage(PaymentViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (BindingContext is PaymentViewModel vm)
                vm.ApplyQueryAttributes(query);
        }
    }
}