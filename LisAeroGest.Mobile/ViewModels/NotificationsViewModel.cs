using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;

namespace LisAeroGest.Mobile.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string _statusLabel = "A carregar alertas…";

    [ObservableProperty]
    private string _unreadLabel = "0 novas";

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<NotificationDto> Notifications { get; } = new();

    public NotificationsViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusLabel = "A carregar alertas…";

            var result = await _apiService.GetNotificationsAsync();

            if (!result.Success)
            {
                Notifications.Clear();
                StatusLabel = result.ErrorMessage
                    ?? "Não foi possível carregar os alertas.";
                UnreadLabel = "—";
                return;
            }

            var notifications = result.Data ?? new List<NotificationDto>();

            Notifications.Clear();
            foreach (var n in notifications)
                Notifications.Add(n);

            var unreadResult = await _apiService.GetUnreadNotificationCountAsync();
            var unreadCount = unreadResult.Success ? unreadResult.Data?.Count ?? notifications.Count(n => !n.IsRead) : notifications.Count(n => !n.IsRead);
            UnreadLabel = $"{unreadCount} nova{(unreadCount == 1 ? "" : "s")}";

            StatusLabel = notifications.Count == 0
                ? "Estás a par de tudo."
                : "Mantém-te informado sobre os teus voos.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task MarkAsReadAsync(NotificationDto notification)
    {
        if (notification == null || notification.IsRead) return;

        var result = await _apiService.MarkNotificationReadAsync(notification.Id);
        if (result.Success)
        {
            notification.IsRead = true;
            RefreshUnreadCount();
        }
    }

    private void RefreshUnreadCount()
    {
        var count = Notifications.Count(n => !n.IsRead);
        UnreadLabel = $"{count} nova{(count == 1 ? "" : "s")}";
    }
}
