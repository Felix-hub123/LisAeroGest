using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Data.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        // Utilizador que realizou a operação
        [MaxLength(450)]
        public string? UserId { get; set; }

        public User? User { get; set; }


        // Tipo de ação realizada
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;


        // Área onde ocorreu a ação
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;


        // Descrição legível da operação
        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;


        // Voo relacionado, quando aplicável
        public int? FlightId { get; set; }

        public Flight? Flight { get; set; }


        // Bilhete relacionado, quando aplicável
        public int? TicketId { get; set; }

        public Ticket? Ticket { get; set; }


        // Informação anterior à alteração
        [MaxLength(500)]
        public string? OldValue { get; set; }


        // Informação depois da alteração
        [MaxLength(500)]
        public string? NewValue { get; set; }


        // Data/hora da operação
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}