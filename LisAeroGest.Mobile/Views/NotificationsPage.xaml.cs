using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.ViewModels;

namespace LisAeroGest.Mobile.Views;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationsViewModel _viewModel;

    public NotificationsPage(
        NotificationsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel
            .LoadCommand
            .ExecuteAsync(null);
    }

    private async void OnRefreshing(
        object sender,
        EventArgs e)
    {
        try
        {
            await _viewModel
                .LoadCommand
                .ExecuteAsync(null);
        }
        finally
        {
            Refresh.IsRefreshing = false;
        }
    }

    private async void OnNotificationSelected(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault()
            is not NotificationDto notification)
        {
            return;
        }

        if (!notification.IsRead)
        {
            await _viewModel
                .MarkAsReadCommand
                .ExecuteAsync(notification);
        }

        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }
}