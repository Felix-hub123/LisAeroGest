using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LisAeroGest.Data.Entities
{
    public class Seat : IEntity, ISoftDelete
    {
        /// <summary>
        /// Identificador único do assento.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Código alfanumérico do assento (ex: 14A, 02C).
        /// </summary>
        [Required(ErrorMessage = "O código do assento é obrigatório.")]
        [MaxLength(10, ErrorMessage = "O código não pode exceder 10 caracteres.")]
        public string? Code { get; set; }

        /// <summary>
        /// Classe do assento: Economy ou Business.
        /// </summary>
        [Required(ErrorMessage = "A classe do assento é obrigatória.")]
        public string? SeatClass { get; set; }

        /// <summary>
        /// Preço base do assento.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; } = 100.00M;

        /// <summary>
        /// Indica se o assento está disponível para reserva.
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Chave estrangeira para a aeronave a que este assento pertence.
        /// </summary>
        public int AircraftId { get; set; }

        /// <summary>
        /// Navegação para a aeronave proprietária deste assento.
        /// </summary>
        public Aircraft? Aircraft { get; set; }

        /// <summary>
        /// Chave estrangeira para o voo — opcional, usado para bloqueio dinâmico.
        /// </summary>
        public int? FlightId { get; set; }

        /// <summary>
        /// Navegação para o voo associado a este assento.
        /// </summary>
        public Flight? Flight { get; set; }

        /// <summary>
        /// Indica se o assento foi eliminado logicamente.
        /// </summary>
        public bool WasDeleted { get; set; }
    }
}