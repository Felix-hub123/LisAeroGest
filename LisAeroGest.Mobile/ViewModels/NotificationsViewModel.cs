using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;
using System.Collections.ObjectModel;

namespace LisAeroGest.Mobile.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly ApiService _apiService;


    // =========================================================
    // PROPRIEDADES
    // =========================================================

    [ObservableProperty]
    private string _statusLabel =
        "A carregar alertas…";

    [ObservableProperty]
    private string _unreadLabel =
        "0 novas";

    [ObservableProperty]
    private bool _isBusy;


    // =========================================================
    // NOTIFICAÇÕES
    // =========================================================

    public ObservableCollection<NotificationDto>
        Notifications
    { get; } = new();


    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public NotificationsViewModel(
        ApiService apiService)
    {
        _apiService = apiService;
    }


    // =========================================================
    // CARREGAR NOTIFICAÇÕES
    // =========================================================

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            StatusLabel =
                "A carregar alertas…";

            var result =
                await _apiService
                    .GetNotificationsAsync();

            if (!result.Success)
            {
                Notifications.Clear();

                StatusLabel =
                    result.ErrorMessage ??
                    "Não foi possível carregar os alertas.";

                UnreadLabel = "—";

                return;
            }

            var notifications =
                result.Data ??
                new List<NotificationDto>();

            Notifications.Clear();

            foreach (var notification in notifications)
            {
                Notifications.Add(notification);
            }


            // =============================================
            // CONTADOR DE NÃO LIDAS
            // =============================================

            var unreadResult =
                await _apiService
                    .GetUnreadNotificationCountAsync();

            var unreadCount =
                unreadResult.Success
                    ? unreadResult.Data?.Count ??
                      notifications.Count(n => !n.IsRead)
                    : notifications.Count(n => !n.IsRead);

            UnreadLabel =
                $"{unreadCount} nova" +
                $"{(unreadCount == 1 ? "" : "s")}";


            // =============================================
            // ESTADO DA PÁGINA
            // =============================================

            StatusLabel =
                notifications.Count == 0
                    ? "Estás a par de tudo."
                    : "Mantém-te informado sobre os teus voos.";


            // =============================================
            // ATUALIZAR TAB ALERTAS
            // =============================================

            if (Shell.Current is AppShell appShell)
            {
                await appShell
                    .RefreshNotificationBadgeAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }


    // =========================================================
    // MARCAR COMO LIDA
    // =========================================================

    [RelayCommand]
    private async Task MarkAsReadAsync(
        NotificationDto notification)
    {
        if (notification == null ||
            notification.IsRead)
        {
            return;
        }

        var result =
            await _apiService
                .MarkNotificationReadAsync(
                    notification.Id);

        if (!result.Success)
            return;


        // Atualizar objeto local.
        notification.IsRead = true;


        // Atualizar contador da página.
        RefreshUnreadCount();


        // Atualizar contador da TabBar.
        if (Shell.Current is AppShell appShell)
        {
            await appShell
                .RefreshNotificationBadgeAsync();
        }
    }


    // =========================================================
    // ATUALIZAR CONTADOR LOCAL
    // =========================================================

    private void RefreshUnreadCount()
    {
        var count =
            Notifications.Count(
                n => !n.IsRead);

        UnreadLabel =
            $"{count} nova" +
            $"{(count == 1 ? "" : "s")}";
    }
}