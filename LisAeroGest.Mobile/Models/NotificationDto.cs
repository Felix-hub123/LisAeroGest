namespace LisAeroGest.Mobile.Models;

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public string Icon { get; set; } = "bi-bell";
    public string ColorClass { get; set; } = "text-primary";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; } = "Info";
}
