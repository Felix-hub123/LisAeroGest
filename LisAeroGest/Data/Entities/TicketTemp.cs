using System.ComponentModel.DataAnnotations.Schema;

namespace LisAeroGest.Data.Entities
{
    public class TicketTemp : IEntity, ISoftDelete
    {
        /// <summary>
        /// Identificador único da reserva temporária.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Chave estrangeira para o passageiro.
        /// </summary>
        public int PassengerId { get; set; }

        /// <summary>
        /// Navegação para o passageiro.
        /// </summary>
        public Passenger? Passenger { get; set; }

        /// <summary>
        /// Chave estrangeira para o voo selecionado.
        /// </summary>
        public int FlightId { get; set; }

        /// <summary>
        /// Navegação para o voo selecionado.
        /// </summary>
        public Flight? Flight { get; set; }

        /// <summary>
        /// Chave estrangeira para o assento bloqueado temporariamente.
        /// </summary>
        public int SeatId { get; set; }

        /// <summary>
        /// Navegação para o assento bloqueado.
        /// </summary>
        public Seat? Seat { get; set; }

        /// <summary>
        /// Preço do bilhete no momento da reserva temporária.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Indica se foi pedida bagagem extra.
        /// </summary>
        public bool ExtraLuggage { get; set; }

        /// <summary>
        /// Indica se foi pedida refeição de bordo.
        /// </summary>
        public bool MealIncluded { get; set; }

        /// <summary>
        /// Identificador do utilizador que iniciou a reserva.
        /// </summary>
        public string? CreatedByUserId { get; set; }

        /// <summary>
        /// Data de criação da reserva temporária.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data limite até à qual a reserva é válida.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Indica se a reserva temporária foi eliminada logicamente.
        /// </summary>
        public bool WasDeleted { get; set; }

    }
}
