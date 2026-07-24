namespace LisAeroGest.Models
{
    public class NotificationViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? Link { get; set; }

        public string Icon { get; set; } = "bi-bell";

        public string ColorClass { get; set; } = "text-primary";

        public string Type { get; set; } = "System";

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
