using System.ComponentModel.DataAnnotations.Schema;

namespace LisAeroGest.Data.Entities
{
    public class Ticket : IEntity, ISoftDelete
    {
        public int Id { get; set; }

        public int PassengerId { get; set; }
        public Passenger? Passenger { get; set; }

        public int FlightId { get; set; }
        public Flight? Flight { get; set; }

        public int SeatId { get; set; }
        public Seat? Seat { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public bool ExtraLuggage { get; set; }
        public bool MealIncluded { get; set; }

        /// <summary>
        /// Estados possíveis: "Reserved", "Paid", "CheckedIn", "Cancelled", "Expired"
        /// </summary>
        public string Status { get; set; } = "Reserved";

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data/hora limite para pagamento da reserva temporária (ex: 15 min após a reserva).
        /// Fica 'null' assim que o status passa a "Paid".
        /// </summary>
        public DateTime? ReservationExpiresAt { get; set; }

        public DateTime? CheckInDate { get; set; }

        public string? CreatedByUserId { get; set; }

        #region Propriedades Utilitárias (Não mapeadas na BD)

        /// <summary>
        /// Retorna true se o passageiro já fez check-in.
        /// </summary>
        [NotMapped]
        public bool HasCheckedIn => Status == "CheckedIn" || CheckInDate.HasValue;

        /// <summary>
        /// Retorna true se a reserva ainda é válida para pagamento.
        /// </summary>
        [NotMapped]
        public bool IsReservationValid => Status == "Reserved" &&
                                          ReservationExpiresAt.HasValue &&
                                          ReservationExpiresAt.Value > DateTime.UtcNow;

        #endregion

        public bool WasDeleted { get; set; }
    }
}
