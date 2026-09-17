using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views
{
    public partial class PaymentPage : ContentPage, IQueryAttributable
    {
        public PaymentPage()
        {
            InitializeComponent();

            var services = Application.Current?.Handler?.MauiContext?.Services;
            if (services != null)
                BindingContext = services.GetService<PaymentViewModel>();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (BindingContext is PaymentViewModel vm)
                vm.ApplyQueryAttributes(query);
        }
    }
}