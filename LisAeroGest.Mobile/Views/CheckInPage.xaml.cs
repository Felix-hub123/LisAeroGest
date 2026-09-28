using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class CheckInPage : ContentPage
{
    private readonly CheckInViewModel _viewModel;

    public CheckInPage(
        CheckInViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}