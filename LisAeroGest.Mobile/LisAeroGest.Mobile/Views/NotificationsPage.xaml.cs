using LisAeroGest.Mobile.Models;
using LisAeroGest.Mobile.Services;

namespace LisAeroGest.Mobile.Views;

public partial class NotificationsPage : ContentPage
{
    private readonly ApiService _apiService;

    public NotificationsPage() : this(ResolveApiService()) { }

    public NotificationsPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private static ApiService ResolveApiService()
        => Application.Current?.Handler?.MauiContext?.Services.GetService<ApiService>()
           ?? throw new InvalidOperationException("ApiService não está registado.");

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadAsync();
        Refresh.IsRefreshing = false;
    }

    private async Task LoadAsync()
    {
        StatusLabel.Text = "A carregar alertas…";
        var result = await _apiService.GetNotificationsAsync();
        if (!result.Success)
        {
            NotificationsView.ItemsSource = Array.Empty<NotificationDto>();
            StatusLabel.Text = result.ErrorMessage ?? "Não foi possível carregar os alertas.";
            UnreadLabel.Text = "—";
            return;
        }

        var notifications = result.Data ?? new List<NotificationDto>();
        NotificationsView.ItemsSource = notifications;
        UnreadLabel.Text = $"{notifications.Count(n => !n.IsRead)} novas";
        StatusLabel.Text = notifications.Count == 0
            ? "Estás a par de tudo."
            : "Mantém-te informado sobre os teus voos.";
    }
}
