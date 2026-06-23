using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LisAeroGest.Data.Entities
{
    public class Flight : IEntity, ISoftDelete
    {
        /// <summary>
        /// Identificador único do voo.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Código comercial do voo (ex: TP1402, FR2241).
        /// </summary>
        [Required(ErrorMessage = "O número do voo é obrigatório.")]
        [MaxLength(10, ErrorMessage = "O número do voo não pode exceder 10 caracteres.")]
        public string? FlightNumber { get; set; }

        /// <summary>
        /// Chave estrangeira para a companhia aérea que opera o voo.
        /// </summary>
        [Required]
        public int AirlineId { get; set; }

        /// <summary>
        /// Navegação para a companhia aérea.
        /// </summary>
        public Airline? Airline { get; set; }

        /// <summary>
        /// Chave estrangeira para o aeroporto de origem.
        /// </summary>
        [Required]
        public int OriginAirportId { get; set; }

        /// <summary>
        /// Navegação para o aeroporto de origem.
        /// </summary>
        public Airport? OriginAirport { get; set; }

        /// <summary>
        /// Chave estrangeira para o aeroporto de destino.
        /// </summary>
        [Required]
        public int DestinationAirportId { get; set; }

        /// <summary>
        /// Navegação para o aeroporto de destino.
        /// </summary>
        public Airport? DestinationAirport { get; set; }

        /// <summary>
        /// Chave estrangeira para a aeronave designada para este voo.
        /// </summary>
        [Required]
        public int AircraftId { get; set; }

        /// <summary>
        /// Navegação para a aeronave.
        /// </summary>
        public Aircraft? Aircraft { get; set; }

        /// <summary>
        /// Chave estrangeira para o gate de embarque — opcional.
        /// </summary>
        public int? GateId { get; set; }

        /// <summary>
        /// Navegação para o gate de embarque.
        /// </summary>
        public Gate? Gate { get; set; }

        /// <summary>
        /// Data e hora de partida do voo.
        /// </summary>
        [Required(ErrorMessage = "A data e hora de partida são obrigatórias.")]
        public DateTime DepartureTime { get; set; }

        /// <summary>
        /// Data e hora de chegada prevista do voo.
        /// </summary>
        [Required(ErrorMessage = "A data e hora de chegada são obrigatórias.")]
        public DateTime ArrivalTime { get; set; }

        /// <summary>
        /// Preço base do bilhete para este voo.
        /// </summary>
        [Required(ErrorMessage = "O preço base é obrigatório.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Estado atual do voo: Scheduled, CheckIn, Boarding, Departed, Delayed, Cancelled.
        /// </summary>
        public string Status { get; set; } = "Scheduled";

        /// <summary>
        /// Nova hora de partida em caso de atraso — opcional.
        /// </summary>
        public DateTime? DelayedDepartureTime { get; set; }

        /// <summary>
        /// Lista de assentos disponíveis neste voo.
        /// </summary>
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();

        /// <summary>
        /// Indica se o voo foi eliminado logicamente.
        /// </summary>
        public bool WasDeleted { get; set; }
    }
}