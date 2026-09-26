using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Data.Entities
{
    public class FlightCommunication
    {
        public int Id { get; set; }

        public int FlightId { get; set; }

        public Flight? Flight { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "Info";

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? SentByUserId { get; set; }

        public User? SentByUser { get; set; }

        public int RecipientsCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}