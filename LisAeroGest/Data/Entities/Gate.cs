using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Data.Entities
{
    public class Gate : IEntity, ISoftDelete
    {
        /// <summary>
        /// Identificador único do gate.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Número ou código do gate (ex: A12, B03).
        /// </summary>
        [Required(ErrorMessage = "O número do gate é obrigatório.")]
        [MaxLength(10, ErrorMessage = "O número do gate não pode exceder 10 caracteres.")]
        public string? GateNumber { get; set; }

        /// <summary>
        /// Terminal onde o gate está localizado (ex: Terminal 1, Terminal 2).
        /// </summary>
        [Required(ErrorMessage = "O terminal é obrigatório.")]
        [MaxLength(50, ErrorMessage = "O terminal não pode exceder 50 caracteres.")]
        public string? Terminal { get; set; }

        /// <summary>
        /// Estado atual do gate: Disponível, Ocupado, Em Manutenção.
        /// </summary>
        [Required]
        public string Status { get; set; } = "Available";

        /// <summary>
        /// Indica se o gate foi eliminado logicamente.
        /// </summary>
        public bool WasDeleted { get; set; }
    }
}