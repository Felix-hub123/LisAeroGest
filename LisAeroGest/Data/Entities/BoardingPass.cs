using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LisAeroGest.Data.Entities
{
    public class BoardingPass
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        public int SequenceNumber { get; set; }

        /// <summary>
        /// Portão de embarque atribuído.
        /// </summary>
        public string? Gate { get; set; } = "TBA";

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(200)]
        public string QRCode { get; set; } = string.Empty;

        /// <summary>
        /// Propriedade utilitária para obter a URL do QR Code de forma dinâmica.
        /// </summary>
        [NotMapped]
        public string QRCodeBase64 => $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=TICKET:{TicketId}|SEQ:{SequenceNumber}";
    }
}
