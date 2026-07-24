using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Data.Entities
{
    public class Notification
    {
        public int Id { get; set; }


        [Required]

        public string UserId { get; set; } = string.Empty;

        public User? User { get; set; }


        [Required, MaxLength(200)]

        public string Title { get; set; } = string.Empty;


        [Required, MaxLength(500)]

        public string Message { get; set; } = string.Empty;


        [MaxLength(300)]

        public string? Link { get; set; }


        [MaxLength(50)]

        public string Icon { get; set; } = "bi-bell";


        [MaxLength(30)]

        public string ColorClass { get; set; } = "text-primary";


        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        [MaxLength(50)]

        public string Type { get; set; } = "Info";
    }
}
