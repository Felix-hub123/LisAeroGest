using System.ComponentModel.DataAnnotations;

namespace LisAeroGest.Models
{
    /// <summary>
    /// Uma fila do mapa da cabine.
    /// </summary>
    public class SeatMapRowViewModel
    {
        [Display(Name = "Fila")]
        public int RowNumber { get; set; }

        [Display(Name = "Executiva")]
        public bool IsBusinessRow { get; set; }

        /// <summary>
        /// Lugares do lado esquerdo (A B C).
        /// </summary>
        public List<SeatMapCellViewModel> LeftSeats { get; set; } = new();

        /// <summary>
        /// Lugares do lado direito (D E F).
        /// </summary>
        public List<SeatMapCellViewModel> RightSeats { get; set; } = new();
    }
}