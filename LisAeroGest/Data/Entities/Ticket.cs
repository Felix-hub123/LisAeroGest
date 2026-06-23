using System.ComponentModel.DataAnnotations.Schema;

namespace LisAeroGest.Data.Entities
{
    public class Ticket : IEntity, ISoftDelete
    {
        /// <summary>
        /// Identificador único do bilhete.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Chave estrangeira para o passageiro titular do bilhete.
        /// </summary>
        public int PassengerId { get; set; }

        /// <summary>
        /// Navegação para o passageiro titular.
        /// </summary>
        public Passenger? Passenger { get; set; }

        /// <summary>
        /// Chave estrangeira para o voo associado ao bilhete.
        /// </summary>
        public int FlightId { get; set; }

        /// <summary>
        /// Navegação para o voo associado.
        /// </summary>
        public Flight? Flight { get; set; }

        /// <summary>
        /// Chave estrangeira para o assento reservado.
        /// </summary>
        public int SeatId { get; set; }

        /// <summary>
        /// Navegação para o assento reservado.
        /// </summary>
        public Seat? Seat { get; set; }

        /// <summary>
        /// Valor total pago pelo bilhete.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Indica se foi contratada bagagem extra.
        /// </summary>
        public bool ExtraLuggage { get; set; }

        /// <summary>
        /// Indica se foi contratada refeição de bordo.
        /// </summary>
        public bool MealIncluded { get; set; }

        /// <summary>
        /// Estado atual do bilhete: Reserved, Paid, CheckedIn, Cancelled.
        /// </summary>
        public string Status { get; set; } = "Reserved";

        /// <summary>
        /// Data e hora em que o bilhete foi comprado.
        /// </summary>
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data e hora em que o check-in foi realizado — opcional.
        /// </summary>
        public DateTime? CheckInDate { get; set; }

        /// <summary>
        /// Identificador do utilizador que criou o bilhete.
        /// </summary>
        public string? CreatedByUserId { get; set; }

        /// <summary>
        /// Indica se o bilhete foi eliminado logicamente.
        /// </summary>
        public bool WasDeleted { get; set; }
    }
}
