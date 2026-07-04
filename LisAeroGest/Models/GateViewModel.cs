using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// ViewModel para criação e edição de gates de embarque.
    /// </summary>
    public class GateViewModel
    {
        /// <summary>
        /// Identificador único do gate — zero para criação, maior que zero para edição.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Número ou código do gate (ex: A01, B03).
        /// </summary>
        [Required(ErrorMessage = "O número do gate é obrigatório.")]
        [MaxLength(10, ErrorMessage = "O número do gate não pode exceder 10 caracteres.")]
        [Display(Name = "Número do Gate")]
        public string? GateNumber { get; set; }

        /// <summary>
        /// Terminal onde o gate está localizado.
        /// </summary>
        [Required(ErrorMessage = "O terminal é obrigatório.")]
        [MaxLength(50, ErrorMessage = "O terminal não pode exceder 50 caracteres.")]
        [Display(Name = "Terminal")]
        public string? Terminal { get; set; }

        /// <summary>
        /// Estado atual do gate: Available, Occupied ou Maintenance.
        /// </summary>
        [Required(ErrorMessage = "O estado é obrigatório.")]
        [Display(Name = "Estado")]
        public string Status { get; set; } = "Available";
    }
}
