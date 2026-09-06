using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Um lugar (ou espaço vazio) no mapa da cabine.
    /// </summary>
    public class SeatMapCellViewModel
    {
        public int? SeatId { get; set; }

        [Display(Name = "Código")]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Letra")]
        public string Letter { get; set; } = string.Empty;

        [Display(Name = "Classe")]
        public string SeatClass { get; set; } = "Economy";

        [Display(Name = "Preço")]
        public decimal Price { get; set; }

        [Display(Name = "Ocupado")]
        public bool IsOccupied { get; set; }

        [Display(Name = "Existe")]
        public bool Exists { get; set; }

        /// <summary>
        /// Classe CSS do botão (seat-available, seat-business, seat-occupied).
        /// </summary>
        public string CssClass { get; set; } = "seat-empty";
    }
}